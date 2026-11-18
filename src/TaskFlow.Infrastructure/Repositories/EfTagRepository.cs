
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class EfTagRepository(TaskFlowDbContext db) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<Tag?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return await db.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Tag?> GetByNameAsync(
        string name,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);

        return await db.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Name == name.Trim(),
                ct);
    }

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        if (ids.Count == 0)
            return Array.Empty<Tag>();

        return await db.Tags
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TagSummary>> GetSummariesAsync(
        CancellationToken ct = default)
    {
        return await db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagSummary(
                t.Id,
                t.Name,
                t.Tasks.Count))
            .ToListAsync(ct);
    }

    public async Task AddAsync(
        Tag tag,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tag);

        db.Tags.Add(tag);
        await db.SaveChangesAsync(ct);
    }
}
