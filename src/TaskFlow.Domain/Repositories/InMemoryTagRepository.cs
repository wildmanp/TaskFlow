using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryTagRepository : ITagRepository
{
    private readonly List<Tag> _tags = [];
    private readonly Lock _sync = new();
    private int _nextId = 1;

    public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IReadOnlyList<Tag> snapshot = _tags
                .OrderBy(t => t.Name)
                .ToList();

            return Task.FromResult(snapshot);
        }
    }

    public Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_tags.FirstOrDefault(t => t.Id == id));
        }
    }

    public Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var tag = _tags.FirstOrDefault(t =>
                string.Equals(t.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(tag);
        }
    }

    public Task AddAsync(Tag tag, CancellationToken ct = default)
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