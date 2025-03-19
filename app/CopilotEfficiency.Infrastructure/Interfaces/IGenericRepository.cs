
namespace CopilotEfficiency.Infrastructure.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task<T?> GetAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(string id, T entity, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    Task<IEnumerable<T?>> GetAllAsync(CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken);
}