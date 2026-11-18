using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Abstractions;

public sealed record TaskStatistics(
    int TotalTasks,
    IReadOnlyDictionary<TaskItemStatus, int> TasksByStatus,
    IReadOnlyDictionary<TaskPriority, int> TasksByPriority,
    int OverdueTasks);