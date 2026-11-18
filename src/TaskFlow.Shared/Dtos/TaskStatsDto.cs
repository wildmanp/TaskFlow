namespace TaskFlow.Shared.Dtos;

public sealed record TaskStatsDto(
    int TotalTasks,
    IReadOnlyList<TaskStatusCountDto> TasksByStatus,
    IReadOnlyList<TaskPriorityCountDto> TasksByPriority,
    int OverdueTasks);