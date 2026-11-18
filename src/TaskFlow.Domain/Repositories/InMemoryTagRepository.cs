
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryTagRepository : ITagRepository
{
    private readonly List<Tag> _tags = [];
    private readonly Lock _sync = new();
    private int _nextId = 1;

    public Task<IReadOnlyList<Tag>> GetAllAsync(
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IReadOnlyList<Tag> result = _tags
                .OrderBy(t => t.Name)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<Tag?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(
                _tags.FirstOrDefault(t => t.Id == id));
        }
    }

    public Task<Tag?> GetByNameAsync(
        string name,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var tag = _tags.FirstOrDefault(t =>
                string.Equals(
                    t.Name,
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(tag);
        }
    }

    public Task<IReadOnlyList<Tag>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var idSet = ids.ToHashSet();

            IReadOnlyList<Tag> result = _tags
                .Where(t => idSet.Contains(t.Id))
                .OrderBy(t => t.Name)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<TagSummary>> GetSummariesAsync(
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IReadOnlyList<TagSummary> result = _tags
                .OrderBy(t => t.Name)
                .Select(t => new TagSummary(
                    t.Id,
                    t.Name,
                    t.Tasks.Count))
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task AddAsync(
        Tag tag,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            tag.Id = _nextId++;
            _tags.Add(tag);
        }

        return Task.CompletedTask;
    }
}
