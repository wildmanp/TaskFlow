
namespace TaskFlow.Domain.Services;

public class TaskExportRow
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Priority { get; init; } = "";
    public string Status { get; init; } = "";
    public string? DueDate { get; init; }
}
