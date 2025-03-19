namespace CopilotEfficiency.Business.Models;

public class RepositoryPullRequestDto
{
    public required string JsonUrl { get; set; }
    public required string RepositoryId { get; set; }
    public required string RepositoryName { get; set; }
    public required string RepositoryUrl { get; set; }
    public required string ProjectId { get; set; }
    public required string ProjectName { get; set; }
    public long PullRequestId { get; set; }
    public required string PullRequestStatus { get; set; }
    public required string PullRequestTitle { get; set; }
    public required string AuthorId { get; set; }
    public required string AuthorDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public required string ClosedById { get; set; }
    public required string ClosedByDisplayName { get; set; }
    public int Comments { get; set; }
    public int Commits { get; set; }
    public int ChangedFiles { get; set; }
}
