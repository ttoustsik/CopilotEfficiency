using CopilotEfficiency.Business.Interfaces;

namespace CopilotEfficiency.Console;

public class UserInteractionService(ICollectRepositoryDataService collectRepositoryDataService)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        System.Console.WriteLine("Welcome to Repository Data Collector!");
        System.Console.WriteLine("Please select repository type:");
        System.Console.WriteLine("1. GitHub");
        System.Console.WriteLine("2. Azure DevOps");
        
        var choice = System.Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                await CollectGitHubDataAsync(cancellationToken);
                break;
            case "2":
                await CollectAzureDevOpsDataAsync(cancellationToken);
                break;
            default:
                System.Console.WriteLine("Invalid selection. Exiting...");
                break;
        }
    }
    
    private async Task CollectGitHubDataAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine("GitHub Repository Data Collection");
        System.Console.WriteLine("--------------------------------");
        
        System.Console.Write("Enter your GitHub access token: ");
        var accessToken = System.Console.ReadLine()!.Trim();
        
        System.Console.Write("Enter repository owner: ");
        var owner = System.Console.ReadLine()!.Trim();

        System.Console.Write("Enter repository name: ");
        var repositoryName = System.Console.ReadLine()!.Trim();

        if (string.IsNullOrWhiteSpace(accessToken) || 
            string.IsNullOrWhiteSpace(owner) || 
            string.IsNullOrWhiteSpace(repositoryName))
        {
            System.Console.WriteLine("Error: All fields are required. Operation cancelled.");
            return;
        }
        
        System.Console.WriteLine($"Collecting data from GitHub repository: {owner}/{repositoryName}...");
        await collectRepositoryDataService.CollectGitHubDataAsync(accessToken, owner, repositoryName, cancellationToken);
        System.Console.WriteLine("Data collection completed successfully.");
    }
    
    private async Task CollectAzureDevOpsDataAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine("Azure DevOps Repository Data Collection");
        System.Console.WriteLine("--------------------------------------");
        
        System.Console.Write("Enter your Azure DevOps access token: ");
        var accessToken = System.Console.ReadLine();
        
        System.Console.Write("Enter Azure DevOps URI: ");
        var uri = System.Console.ReadLine();
        
        System.Console.Write("Enter project name: ");
        var project = System.Console.ReadLine();
        
        System.Console.Write("Enter repository name: ");
        var repository = System.Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(accessToken) || 
            string.IsNullOrWhiteSpace(uri) || 
            string.IsNullOrWhiteSpace(project) || 
            string.IsNullOrWhiteSpace(repository))
        {
            System.Console.WriteLine("Error: All fields are required. Operation cancelled.");
            return;
        }
        
        System.Console.WriteLine($"Collecting data from Azure DevOps repository: {project}/{repository}...");
        await collectRepositoryDataService.CollectAzureDevOpsDataAsync(accessToken, uri, project, repository, cancellationToken);
        System.Console.WriteLine("Data collection completed successfully.");
    }
}