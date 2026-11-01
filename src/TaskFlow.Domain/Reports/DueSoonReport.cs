
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Reports;

public class DueSoonReport : TaskReport
{
    public string Title => "Termen în următoarele 7 zile";

    public IReadOnlyList<TaskItem> Tasks { get; init; }
        = Array.Empty<TaskItem>();

    public static DueSoonReport Generate(
        IEnumerable<TaskItem> tasks,
        DateTime now)
    {
        DateTime today = now.Date;
        DateTime limit = today.AddDays(7);

        var filteredTasks = tasks
            .Where(t => t.Status != TaskItemStatus.Done
                        && t.DueDate is not null
                        && t.DueDate.Value.Date >= today
                        && t.DueDate.Value.Date <= limit)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .ToList();

        return new DueSoonReport
        {
            Tasks = filteredTasks,
            TotalTasks = filteredTasks.Count,
            TodoTasks = filteredTasks.Count(
                t => t.Status == TaskItemStatus.Todo),
            InProgressTasks = filteredTasks.Count(
                t => t.Status == TaskItemStatus.InProgress),
            CompletedTasks = 0
        };
    }

    public string FormatTask(TaskItem task, DateTime now)
    {
        int daysRemaining = (task.DueDate!.Value.Date - now.Date).Days;

        return $"{task.Title}, au rămas {daysRemaining} zile";
    }

    public string Render(DateTime now)
    {
        var lines = new List<string> { Title };

        foreach (var task in Tasks)
        {
            lines.Add(FormatTask(task, now));
        }

        return string.Join(Environment.NewLine, lines);
    }
}
