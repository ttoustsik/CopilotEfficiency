namespace CopilotEfficiency.Infrastructure.Interfaces;

public interface IGenericFileWorker
{
    Task SaveToJson<T>(IList<T> list, CancellationToken cancellationToken);
}