using System.Text.Json;
using CopilotEfficiency.Infrastructure.Interfaces;

namespace CopilotEfficiency.Infrastructure.Workers;

public class FileWorker : IGenericFileWorker
{
    public async Task SaveToJson<T>(IList<T> list, CancellationToken cancellationToken)
    {
        if (list == null || !list.Any())
        {
            throw new ArgumentException("List is null or empty", nameof(list));
        }

        var typeName = typeof(T).Name.ToLowerInvariant();
        var fileName = $"{typeName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonData", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var jsonString = JsonSerializer.Serialize(list, options);
        await File.WriteAllTextAsync(filePath, jsonString, cancellationToken);
    }
}