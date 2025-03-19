using CopilotEfficiency.Business.Interfaces;
using CopilotEfficiency.Business.Services;
using CopilotEfficiency.Console;
using CopilotEfficiency.Infrastructure.Interfaces;
using CopilotEfficiency.Infrastructure.Repositories;
using CopilotEfficiency.Infrastructure.Workers;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();

var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("Program");

try
{
    using var cancellationTokenSource = new CancellationTokenSource();
    var interactionService = scope.ServiceProvider.GetRequiredService<UserInteractionService>();

    await interactionService.RunAsync(cancellationTokenSource.Token);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during execution.");
}
finally
{
    logger.LogInformation("Application is shutting down.");
}

IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();

            config.AddJsonFile("appsettings.json", true, true);
        })
        .ConfigureServices((context, services) => ConfigureServices(context.Configuration, services));
}

void ConfigureServices(IConfiguration configuration, IServiceCollection services)
{        
    var elasticsearchSettings = new ElasticsearchClientSettings(new Uri(configuration.GetValue<string>("ElasticsearchSettings:Url") ?? string.Empty))
        .DefaultIndex(configuration.GetValue<string>("ElasticsearchSettings:DefaultIndex") ?? string.Empty)
        .Authentication(new BasicAuthentication(
            configuration.GetValue<string>("ElasticsearchSettings:Username") ?? string.Empty,
            configuration.GetValue<string>("ElasticsearchSettings:Password") ?? string.Empty))
        .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true) 
        .DisableDirectStreaming(); 

    var elasticClient = new ElasticsearchClient(elasticsearchSettings);
    services.AddSingleton(elasticClient);

    services.AddScoped(typeof(IGenericRepository<>), typeof(ElasticRepository<>));
    services.AddScoped<IGenericFileWorker, FileWorker>();
    services.AddScoped<IGitHubService, GitHubService>();
    services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
    services.AddScoped<ICollectRepositoryDataService, CollectRepositoryDataService>();
    services.AddScoped<UserInteractionService>();

    services.AddAutoMapper(typeof(GitHubService).Assembly);
}