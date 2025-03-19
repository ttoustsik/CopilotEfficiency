using CopilotEfficiency.Business.Models;

namespace CopilotEfficiency.Business.Interfaces;

public interface IAzureDevOpsService
{
    Task<List<RepositoryPullRequestDto>> GetAllPullRequestsData(string accessToken, string uri, string project, string repository, CancellationToken cancellationToken);
    Task SavePullRequestsToDb(List<RepositoryPullRequestDto> pullRequests, CancellationToken cancellationToken);
    Task SavePullRequestsToJson(List<RepositoryPullRequestDto> pullRequests, CancellationToken cancellationToken);
}
