
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(
        CancellationToken ct = default);

    Task<IReadOnlyList<TaskItem>> SearchAsync(
        TaskFilter filter,
        CancellationToken ct = default);

    Task<TaskItem?> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task AddAsync(
        TaskItem task,
        CancellationToken ct = default);

    Task UpdateAsync(
        TaskItem task,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default);

    Task<TaskStatistics> GetStatisticsAsync(
    DateTime now,
    CancellationToken ct = default);

    Task<bool> ReplaceTagsAsync(
        int taskId,
        IReadOnlyCollection<int> tagIds,
        CancellationToken ct = default);
}
