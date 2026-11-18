
namespace TaskFlow.Domain.Abstractions;

public interface ITagService
{
    Task<IReadOnlyList<TagSummary>> GetAllAsync(
        CancellationToken ct = default);

    Task<TagSummary?> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task<TagSummary> CreateAsync(
        TaskFlow.Domain.Entities.Tag tag,
        CancellationToken ct = default);
}
