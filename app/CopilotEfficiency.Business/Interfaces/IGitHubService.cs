using CopilotEfficiency.Business.Models;

namespace CopilotEfficiency.Business.Interfaces;

public interface IGitHubService
{
    Task<List<RepositoryPullRequestDto>> GetAllPullRequestsData(string accessToken, string owner, string repoName, CancellationToken cancellationToken);
    Task SavePullRequestsToDb(List<RepositoryPullRequestDto> pullRequests, CancellationToken cancellationToken);
    Task SavePullRequestsToJson(List<RepositoryPullRequestDto> pullRequests, CancellationToken cancellationToken);
}
