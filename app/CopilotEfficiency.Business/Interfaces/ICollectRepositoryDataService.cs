namespace CopilotEfficiency.Business.Interfaces;

public interface ICollectRepositoryDataService
{
    Task CollectGitHubDataAsync(string accessToken, string owner, string repoName, CancellationToken cancellationToken);
    Task CollectAzureDevOpsDataAsync(string accessToken, string uri, string project, string repository, CancellationToken cancellationToken);
}

