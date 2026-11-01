
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Reports;

public class TaskReport
{
    public int TotalTasks { get; init; }
    public int TodoTasks { get; init; }
    public int InProgressTasks { get; init; }
    public int CompletedTasks { get; init; }

    public static TaskReport Generate(IEnumerable<TaskItem> tasks)
    {
        var items = tasks.ToList();

        return new TaskReport
        {
            TotalTasks = items.Count,
            TodoTasks = items.Count(t => t.Status == Enums.TaskItemStatus.Todo),
            InProgressTasks = items.Count(t => t.Status == Enums.TaskItemStatus.InProgress),
            CompletedTasks = items.Count(t => t.Status == Enums.TaskItemStatus.Done)
        };
    }
}

