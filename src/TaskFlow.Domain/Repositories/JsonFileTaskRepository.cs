
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class JsonFileTaskRepository : ITaskRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private readonly InMemoryTaskRepository _inner;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonFileTaskRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(
                "Calea fișierului este obligatorie.",
                nameof(filePath));

        _filePath = filePath;
        _inner = new InMemoryTaskRepository(Load());
    }

    public Task<IReadOnlyList<TaskItem>> GetAllAsync(
        CancellationToken ct = default)
    {
        return _inner.GetAllAsync(ct);
    }

    public Task<IReadOnlyList<TaskItem>> SearchAsync(
        TaskFilter filter,
        CancellationToken ct = default)
    {
        return _inner.SearchAsync(filter, ct);
    }

    public Task<TaskItem?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return _inner.GetByIdAsync(id, ct);
    }

    public Task<TaskStatistics> GetStatisticsAsync(
        DateTime now,
        CancellationToken ct = default)
    {
        return _inner.GetStatisticsAsync(now, ct);
    }

    public async Task AddAsync(
        TaskItem task,
        CancellationToken ct = default)
    {
        await _inner.AddAsync(task, ct);
        await SaveAsync(ct);
    }

    public async Task UpdateAsync(
        TaskItem task,
        CancellationToken ct = default)
    {
        await _inner.UpdateAsync(task, ct);
        await SaveAsync(ct);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        bool deleted = await _inner.DeleteAsync(id, ct);

        if (deleted)
            await SaveAsync(ct);

        return deleted;
    }

    public async Task<bool> ReplaceTagsAsync(
        int taskId,
        IReadOnlyCollection<int> tagIds,
        CancellationToken ct = default)
    {
        bool replaced = await _inner.ReplaceTagsAsync(taskId, tagIds, ct);

        if (replaced)
            await SaveAsync(ct);

        return replaced;
    }

    private List<TaskItem> Load()
    {
        if (!File.Exists(_filePath))
            return new List<TaskItem>();

        string json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<TaskItem>();

        return JsonSerializer.Deserialize<List<TaskItem>>(
            json,
            JsonOptions) ?? new List<TaskItem>();
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);

        try
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            IReadOnlyList<TaskItem> tasks =
                await _inner.GetAllAsync(ct);

            await using FileStream stream = File.Create(_filePath);

            await JsonSerializer.SerializeAsync(
                stream,
                tasks,
                JsonOptions,
                ct);
        }
        finally
        {
            _gate.Release();
        }
    }
}
