using CopilotEfficiency.Business.Interfaces;

namespace CopilotEfficiency.Business.Services;

public class CollectRepositoryDataService(
    IGitHubService gitHubService,
    IAzureDevOpsService azureDevOpsService) : ICollectRepositoryDataService
{
    public async Task CollectGitHubDataAsync(string accessToken, string owner, string repoName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new ArgumentNullException(nameof(accessToken), "Access token is null or empty.");
        }

        if (string.IsNullOrEmpty(owner))
        {
            throw new ArgumentNullException(nameof(owner), "Owner is null or empty.");
        }

        if (string.IsNullOrEmpty(repoName))
        {
            throw new ArgumentNullException(nameof(repoName), "Repository name is null or empty.");
        }

        var pullRequests = await gitHubService.GetAllPullRequestsData(accessToken, owner, repoName, cancellationToken);

        await gitHubService.SavePullRequestsToDb(pullRequests, cancellationToken);
        await gitHubService.SavePullRequestsToJson(pullRequests, cancellationToken);
    }

    public async Task CollectAzureDevOpsDataAsync(string accessToken, string uri, string project, string repository, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new ArgumentNullException(nameof(accessToken), "Access token is null or empty.");
        }

        if (string.IsNullOrEmpty(project))
        {
            throw new ArgumentNullException(nameof(project), "Project is null or empty.");
        }

        if (string.IsNullOrEmpty(repository))
        {
            throw new ArgumentNullException(nameof(repository), "Repository name is null or empty.");
        }

        if (string.IsNullOrEmpty(uri))
        {
            throw new ArgumentNullException(nameof(uri), "Uri is null or empty.");
        }

        var pullRequests = await azureDevOpsService.GetAllPullRequestsData(accessToken, uri, project, repository, cancellationToken);

        await azureDevOpsService.SavePullRequestsToDb(pullRequests, cancellationToken);
        await azureDevOpsService.SavePullRequestsToJson(pullRequests, cancellationToken);
    }
}
