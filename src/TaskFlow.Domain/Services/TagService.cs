
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Services;

public sealed class TagService(ITagRepository repository) : ITagService
{
    public Task<IReadOnlyList<TagSummary>> GetAllAsync(
        CancellationToken ct = default)
    {
        return repository.GetSummariesAsync(ct);
    }

    public async Task<TagSummary?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        var summaries = await repository.GetSummariesAsync(ct);

        return summaries.FirstOrDefault(t => t.Id == id);
    }

    public async Task<TagSummary> CreateAsync(
        Tag tag,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var existing = await repository.GetByNameAsync(tag.Name, ct);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"A tag named '{tag.Name}' already exists.");
        }

        await repository.AddAsync(tag, ct);

        return new TagSummary(tag.Id, tag.Name, 0);
    }
}
