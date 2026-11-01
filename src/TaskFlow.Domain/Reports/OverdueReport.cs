
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Reports;

public class OverdueReport
{
    public IReadOnlyList<TaskItem> Tasks { get; init; } = Array.Empty<TaskItem>();

    public static OverdueReport Generate(IEnumerable<TaskItem> tasks, DateTime now)
    {
        return new OverdueReport
        {
            Tasks = tasks.Where(t => t.IsOverdue(now)).ToList()
        };
    }
}
