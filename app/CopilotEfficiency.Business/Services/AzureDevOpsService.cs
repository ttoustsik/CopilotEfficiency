using AutoMapper;
using CopilotEfficiency.Business.Interfaces;
using CopilotEfficiency.Business.Models;
using CopilotEfficiency.Core.Entities;
using CopilotEfficiency.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace CopilotEfficiency.Business.Services;

public class AzureDevOpsService(
    IGenericRepository<RepositoryPullRequestEntity> pullRequestRepository, 
    IGenericFileWorker fileWorker, 
    IMapper mapper,
    ILogger<AzureDevOpsService> logger) : IAzureDevOpsService
{
    public async Task<List<RepositoryPullRequestDto>> GetAllPullRequestsData(string accessToken, string uri,
        string project, string repository, CancellationToken cancellationToken)
    {
        var credentials = new VssBasicCredential(string.Empty, accessToken);
        var connection = new VssConnection(new Uri(uri), credentials);
        var client = connection.GetClient<GitHttpClient>();

        var repositories = await client.GetRepositoriesAsync(project, cancellationToken: cancellationToken);

        var repo = repositories.SingleOrDefault(r => r.Name == repository);

        if (repo == null)
        {
            throw new Exception($"Repository {repository} not found in project {project}");
        }

        var searchCriteria = new GitPullRequestSearchCriteria
        {
            Status = PullRequestStatus.All
        };

        var pullRequests = await client.GetPullRequestsAsync(repo.Id, searchCriteria, cancellationToken: cancellationToken);

        var repositoryPullRequestDtos = new List<RepositoryPullRequestDto>();

        logger.LogInformation($"Starting to process {pullRequests.Count} pull requests from {project}/{repository}");

        for (var i = 0; i < pullRequests.Count; i++)
        {
            var pullRequest = pullRequests[i];
            logger.LogInformation($"{i + 1} from {pullRequests.Count} records are processed");

            var repositoryPullRequestDto = mapper.Map<RepositoryPullRequestDto>(pullRequest);
            repositoryPullRequestDto.Comments = await GetCountComments(client, pullRequest, cancellationToken);
            repositoryPullRequestDto.ChangedFiles = await GetCountFiles(client, pullRequest, cancellationToken);
            repositoryPullRequestDto.Commits = await GetCountCommits(client, pullRequest, cancellationToken);

            repositoryPullRequestDtos.Add(repositoryPullRequestDto);
        }

        logger.LogInformation($"Completed processing all {pullRequests.Count} pull requests from {project}/{repository}");
        return repositoryPullRequestDtos;
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

    private static async Task<int> GetCountComments(GitHttpClient client, GitPullRequest pullRequest, CancellationToken cancellationToken)
    {
        var threads = await client.GetThreadsAsync(pullRequest.Repository.Id, pullRequest.PullRequestId, cancellationToken: cancellationToken);

        return threads.Sum(thread => thread.Comments.Count);
    }

    private static async Task<int> GetCountFiles(GitHttpClient client, GitPullRequest pullRequest, CancellationToken cancellationToken)
    {
        var files = 0;
        var iterations = await client.GetPullRequestIterationsAsync(pullRequest.Repository.Id, pullRequest.PullRequestId, cancellationToken: cancellationToken);

        foreach (var iteration in iterations)
        {
            if (iteration?.Id == null)
            {
                continue;
            }

            var changesResponse = await client.GetPullRequestIterationChangesAsync(pullRequest.Repository.Id, pullRequest.PullRequestId, iteration.Id.Value, cancellationToken: cancellationToken);

            files += changesResponse.ChangeEntries
                .Select(change => change.Item.Path)
                .Distinct()
                .Count();
        }

        return files;
    }

    private static async Task<int> GetCountCommits(GitHttpClient client, GitPullRequest pullRequest, CancellationToken cancellationToken)
    {
        var commits = await client.GetPullRequestCommitsAsync(pullRequest.Repository.Id, pullRequest.PullRequestId, cancellationToken: cancellationToken);

        return commits.Count;
    }
}
