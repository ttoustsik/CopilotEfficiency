using CopilotEfficiency.Infrastructure.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace CopilotEfficiency.Infrastructure.Repositories;

public class ElasticRepository<T>(ElasticsearchClient elasticClient) : IGenericRepository<T>
    where T : class
{
    private readonly ElasticsearchClient _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    private readonly string _indexName = typeof(T).Name.ToLowerInvariant();

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.IndexAsync(entity, i => i.Index(_indexName), cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new Exception($"Error adding document: {response.ElasticsearchServerError?.Error.Reason}");
        }
    }
    
    public async Task<T?> GetAsync(string id, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.GetAsync<T>(id, g => g.Index(_indexName), cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode || !response.Found)
        {
            return null;
        }

        return response.Source;
    }
    
    public async Task UpdateAsync(string id, T entity, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<T, T>(id, u => u.Index(_indexName).Doc(entity), cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new Exception($"Error updating document: {response.ElasticsearchServerError?.Error.Reason}");
        }
    }
    
    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.DeleteAsync<T>(id, d => d.Index(_indexName), cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new Exception($"Error deleting document: {response.ElasticsearchServerError?.Error.Reason}");
        }
    }

    public async Task<IEnumerable<T?>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _elasticClient
            .SearchAsync<T>(s => s.Index(_indexName).Query(q => q.MatchAll(new MatchAllQuery())), cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new Exception($"Error getting all documents: {response.ElasticsearchServerError?.Error.Reason}");
        }

        return response.Hits.Select(hit => hit.Source);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await GetAsync(id, cancellationToken);
    }
}