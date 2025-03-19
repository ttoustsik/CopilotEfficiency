using AutoMapper;
using CopilotEfficiency.Business.Interfaces;
using CopilotEfficiency.Business.Models;
using CopilotEfficiency.Core.Entities;
using CopilotEfficiency.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Octokit;

namespace CopilotEfficiency.Business.Services;

public class GitHubService(
    IGenericRepository<RepositoryPullRequestEntity> pullRequestRepository,
    IGenericFileWorker fileWorker,
    IMapper mapper,
    ILogger<GitHubService> logger)
    : IGitHubService
{
    private const int PageSize = 100;
    private const int PageCount = 1;
    private const string Github = "GitHub";

    public async Task<List<RepositoryPullRequestDto>> GetAllPullRequestsData(string accessToken, string owner, string repoName, CancellationToken cancellationToken)
    {
        var client = new GitHubClient(new ProductHeaderValue(Github))
        {
            Credentials = new Credentials(accessToken)
        };

        logger.LogInformation($"Starting to collect all pull requests from {owner}/{repoName}");

        var pullRequests = await CollectPullRequests(owner, repoName, cancellationToken, client);

        logger.LogInformation($"Starting to process pull requests from {owner}/{repoName}");
        
        var pullRequestDtos = await ProcessPullRequests(client, pullRequests, cancellationToken);
        
        logger.LogInformation($"Completed processing all pull requests from {owner}/{repoName}");

        return pullRequestDtos;
    }

    private async Task<List<PullRequest>> CollectPullRequests(string owner, string repoName,
        CancellationToken cancellationToken, GitHubClient client)
    {
        var prRequest = new PullRequestRequest
        {
            State = ItemStateFilter.All
        };

        var allPullRequests = new List<PullRequest>();
        IReadOnlyList<PullRequest> pullRequestsPage;

        var page = 1;

        do
        {
            var options = new ApiOptions
            {
                PageSize = PageSize,
                PageCount = PageCount,
                StartPage = page
            };

            await CheckRateLimits(client, cancellationToken);


            pullRequestsPage = await client.PullRequest.GetAllForRepository(owner, repoName, prRequest, options);
            allPullRequests.AddRange(pullRequestsPage);
            page++;

        } while (pullRequestsPage.Count == PageSize);

        return allPullRequests;
    }

    public async Task SavePullRequestsToDb(List<RepositoryPullRequestDto> pullRequests, CancellationToken cancellationToken)
    {
        foreach (var pr in pullRequests)
        {
            var prRecord = mapper.Map<RepositoryPullRequestEntity>(pr);

            await pullRequestRepository.AddAsync(prRecord, cancellationToken);
        }
    }

    public async Task SavePullRequestsToJson(List<RepositoryPullRequestDto> pullRequests, CancellationToken cancellationToken)
    {
        await fileWorker.SaveToJson(pullRequests, cancellationToken);
    }

    private async Task<List<RepositoryPullRequestDto>> ProcessPullRequests(GitHubClient client, List<PullRequest> pullRequests, CancellationToken cancellationToken)
    {
        var repositoryPullRequestDtos = new List<RepositoryPullRequestDto>();

        for (var i = 0; i < pullRequests.Count; i++)
        {
            var repositoryPullRequestDto = await ProcessPullRequest(client, pullRequests[i], cancellationToken);

            repositoryPullRequestDtos.Add(repositoryPullRequestDto);

            logger.LogInformation($"{i + 1} from {pullRequests.Count} records are processed");
        }

        return repositoryPullRequestDtos;
    }

    private async Task<RepositoryPullRequestDto> ProcessPullRequest(GitHubClient client, PullRequest pullRequest, CancellationToken cancellationToken)
    {
        var repositoryPullRequestDto = mapper.Map<RepositoryPullRequestDto>(pullRequest);

        repositoryPullRequestDto.Comments = await GetCountComments(client, pullRequest, cancellationToken);
        // repositoryPullRequestDto.Commits = await GetCountCommits(client, pullRequest, cancellationToken);
        // repositoryPullRequestDto.ChangedFiles = await GetCountChangedFiles(client, pullRequest, cancellationToken);

        return repositoryPullRequestDto;
    }

    private async Task<int> GetCountComments(GitHubClient client, PullRequest pullRequest, CancellationToken cancellationToken)
    {
        await CheckRateLimits(client, cancellationToken);

        var comments = await client.Issue.Comment.GetAllForIssue(pullRequest.Base.Repository.Id, pullRequest.Number);

        return comments.Count;
    }

    private async Task<int> GetCountCommits(GitHubClient client, PullRequest pullRequest, CancellationToken cancellationToken)
    {
        await CheckRateLimits(client, cancellationToken);

        var commits = await client.PullRequest.Commits(pullRequest.Base.Repository.Id, pullRequest.Number);

        return commits.Count;
    }

    private async Task<int> GetCountChangedFiles(GitHubClient client, PullRequest pullRequest, CancellationToken cancellationToken)
    {
        await CheckRateLimits(client, cancellationToken);

        var files = await client.PullRequest.Files(pullRequest.Base.Repository.Id, pullRequest.Number);
        
        return files.Count;
    }

    private async Task CheckRateLimits(GitHubClient client, CancellationToken cancellationToken)
    {
        try
        {
            var rateLimits = await client.RateLimit.GetRateLimits();
            var coreLimit = rateLimits.Resources.Core;
            
            if (coreLimit.Remaining < coreLimit.Limit * 0.1)
            {
                var timeToReset = coreLimit.Reset.ToLocalTime() - DateTime.Now;
                if (timeToReset.TotalSeconds > 0)
                {
                    logger.LogWarning($"Rate limit approaching threshold. Remaining: {coreLimit.Remaining}/{coreLimit.Limit}. Waiting for {timeToReset.TotalMinutes:F1} minutes until reset at {coreLimit.Reset.ToLocalTime()}");
                    await Task.Delay(timeToReset, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Failed to check rate limits: {ex.Message}. Falling back to counter-based approach.");
            await Task.Delay(TimeSpan.FromMinutes(61), cancellationToken);
        }
    }
}
