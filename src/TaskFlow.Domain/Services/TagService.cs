using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Services;

public sealed class TagService(ITagRepository repository) : ITagService
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        var tags = await repository.GetAllAsync(ct);
        return tags.OrderBy(t => t.Name).ToList();
    }

    public Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default)
        => repository.GetByIdAsync(id, ct);

    public async Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var existing = await repository.GetByNameAsync(tag.Name, ct);

        if (existing is not null)
            throw new InvalidOperationException(
                $"A tag named '{tag.Name}' already exists.");

        await repository.AddAsync(tag, ct);
        return tag;
    }
}