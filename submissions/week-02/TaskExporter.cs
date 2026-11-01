
using System.Text.Json;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Services;

public class TaskExporter
{
    public string Export(IEnumerable<TaskItem> tasks)
    {
        var rows = tasks.Select(task => new TaskExportRow
        {
            Id = task.Id,
            Title = task.Title,
            Priority = task.Priority.ToString(),
            Status = task.Status.ToString(),
            DueDate = task.DueDate?.ToString("yyyy-MM-dd")
        });

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(rows, options);
    }
}
