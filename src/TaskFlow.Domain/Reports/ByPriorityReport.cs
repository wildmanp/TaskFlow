
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Reports;

public class ByPriorityReport
{
    public IReadOnlyDictionary<TaskPriority, int> Counts { get; init; }
        = new Dictionary<TaskPriority, int>();

    public static ByPriorityReport Generate(IEnumerable<TaskItem> tasks)
    {
        return new ByPriorityReport
        {
            Counts = tasks
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
