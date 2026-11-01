using TaskFlow.Domain.Reports;
using TaskFlow.Domain.Services;
using System.Text.Json;
using TaskFlow.ConsoleApp;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Notifications;
using TaskFlow.Domain.Repositories;

bool useMemory = args.Contains("--memory", StringComparer.OrdinalIgnoreCase);

string dataDirectory = Path.Combine(
    Directory.GetCurrentDirectory(), "data");

Directory.CreateDirectory(dataDirectory);

ITaskRepository repository = useMemory
    ? new InMemoryTaskRepository()
    : new JsonFileTaskRepository(
        Path.Combine(dataDirectory, "tasks.json"));

INotifier notifier = new CompositeNotifier(
    new ConsoleNotifier(),
    new FileNotifier(
        Path.Combine(dataDirectory, "notifications.log")));

ITaskService taskService = new TaskService(
    repository,
    notifier,
    TimeProvider.System);

var projects = new Dictionary<int, Project>();
var nextProjectId = 1;

List<TaskItem> GetTasks() => taskService.GetAll().ToList();

Console.WriteLine("=== TaskFlow ===");
Console.WriteLine(useMemory ? "Storage: in-memory" : "Storage: JSON file");

while (true)
{
Console.WriteLine("1. Add task");
Console.WriteLine("2. List tasks");
Console.WriteLine("3. Start task");
Console.WriteLine("4. Complete task");
Console.WriteLine("5. Show overdue tasks");
Console.WriteLine("6. Reopen task");
Console.WriteLine("7. Show task details");
Console.WriteLine("8. Edit task");
Console.WriteLine("9. Create project");
Console.WriteLine("10. List projects");
Console.WriteLine("11. Raport: termen în următoarele 7 zile");
Console.WriteLine("12. Filtru după stare și prioritate");
Console.WriteLine("13. Export JSON");
Console.WriteLine("14. Show statistics");
Console.WriteLine("15. Search tasks");
Console.WriteLine("16. Delete task");
Console.WriteLine("17. Assign task to project");
Console.WriteLine("0. Exit");
Console.Write("Choose: ");

    string choice = Console.ReadLine()?.Trim() ?? "";

    try
    {
        switch (choice)
        {
            case "1": AddFromInput(); break;
            case "2": PrintTasks(GetTasks(), "All tasks"); break;
            case "3": ChangeTaskStatus("start"); break;
            case "4": ChangeTaskStatus("complete"); break;
            case "5":
                PrintTasks(
                    GetTasks().Where(t => t.IsOverdue(
                        TimeProvider.System.GetLocalNow().DateTime)),
                    "Overdue tasks");
                break;
            case "6": ChangeTaskStatus("reopen"); break;
            case "7": ShowTaskDetails(); break;
            case "8": EditTask(); break;
            case "9": CreateProject(); break;
            case "10": ListProjects(); break;
            case "11":
            {
                DateTime now = TimeProvider.System.GetLocalNow().DateTime;
                var report = DueSoonReport.Generate(taskService.GetAll(), now);
                Console.WriteLine(report.Render(now));
                break;
            }
            case "12":
            {
                FilterTasks();
                break;
            }
            case "13":
            {
                ExportTasks();
                break;
            }
            case "14":
            {
                ShowStatistics();
                break;
            }
            case "15":
            {
                SearchTasks();
                break;
            }
            case "16":
            {
                DeleteTask();
                break;
            }
            case "17":
            {
                AssignTaskToProject();
                break;
            }
            case "0":
                Console.WriteLine("Goodbye!");
                return;
            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }
    catch (Exception ex) when (
        ex is ArgumentException ||
        ex is InvalidOperationException ||
        ex is KeyNotFoundException)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

void AddFromInput()
{
    string title = ConsoleInput.ReadText("Title: ");
    TaskPriority priority = ConsoleInput.ReadPriority();
    DateTime? dueDate = ConsoleInput.ReadOptionalDate(
        "Due date (yyyy-MM-dd, empty = none): ");
    string description = ConsoleInput.ReadText("Description (optional): ");

    TaskItem task = taskService.Create(title, priority, dueDate, description);
    Console.WriteLine($"Added: {task}");
}

void PrintTasks(IEnumerable<TaskItem> items, string heading)
{
    List<TaskItem> list = items.ToList();
    Console.WriteLine($"\n--- {heading} ({list.Count}) ---");

    if (list.Count == 0)
    {
        Console.WriteLine("No tasks found.");
        return;
    }

    foreach (TaskItem task in list)
        Console.WriteLine(task);
}

TaskItem? AskForTask()
{
    int id = ConsoleInput.ReadInt("Task ID: ");
    return taskService.GetAll().FirstOrDefault(t => t.Id == id);
}

void ChangeTaskStatus(string action)
{
    TaskItem? task = AskForTask();

    if (task is null)
    {
        Console.WriteLine("Task not found.");
        return;
    }

    switch (action)
    {
        case "start": taskService.Start(task.Id); break;
        case "complete": taskService.Complete(task.Id); break;
        case "reopen": taskService.Reopen(task.Id); break;
    }

    Console.WriteLine($"Updated: {taskService.GetAll().First(t => t.Id == task.Id)}");
}

void ShowTaskDetails()
{
    TaskItem? task = AskForTask();

    if (task is null)
    {
        Console.WriteLine("Task not found.");
        return;
    }

    Console.WriteLine($"ID: {task.Id}");
    Console.WriteLine($"Title: {task.Title}");
    Console.WriteLine($"Description: {task.Description ?? "(none)"}");
    Console.WriteLine($"Priority: {task.Priority}");
    Console.WriteLine($"Status: {task.Status}");
    Console.WriteLine($"Created: {task.CreatedAt}");
    Console.WriteLine($"Due date: {task.DueDate?.ToString("yyyy-MM-dd") ?? "(none)"}");
    Console.WriteLine($"Completed at: {task.CompletedAt?.ToString() ?? "(not completed)"}");
    Console.WriteLine($"Project: {task.Project?.Name ?? "(none)"}");
}

void EditTask()
{
    TaskItem? task = AskForTask();

    if (task is null)
    {
        Console.WriteLine("Task not found.");
        return;
    }

    Console.WriteLine("Leave a field empty to keep its current value.");

    string title = ConsoleInput.ReadText($"Title [{task.Title}]: ");
    if (!string.IsNullOrWhiteSpace(title))
        task.Title = title;

    string description = ConsoleInput.ReadText(
        $"Description [{task.Description ?? ""}]: ");
    if (!string.IsNullOrWhiteSpace(description))
        task.Description = description;

    taskService.Update(task);
    Console.WriteLine($"Task updated: {task}");
}

void CreateProject()
{
    string name = ConsoleInput.ReadText("Project name: ");
    string description = ConsoleInput.ReadText("Description (optional): ");

    Project project = new(name, description) { Id = nextProjectId++ };
    projects.Add(project.Id, project);

    Console.WriteLine($"Created project #{project.Id}: {project.Name}");
}

void ListProjects()
{
    Console.WriteLine("\n--- Projects ---");

    if (projects.Count == 0)
    {
        Console.WriteLine("No projects found.");
        return;
    }

    foreach (Project project in projects.Values)
        Console.WriteLine($"#{project.Id} {project.Name} — {project.Tasks.Count} task(s)");
}

void AssignTaskToProject()
{
    TaskItem? task = AskForTask();

    if (task is null)
    {
        Console.WriteLine("Task not found.");
        return;
    }

    ListProjects();
    int projectId = ConsoleInput.ReadInt("Project ID: ");

    if (!projects.TryGetValue(projectId, out Project? project))
    {
        Console.WriteLine("Project not found.");
        return;
    }

    if (task.Project is not null)
        task.Project.Tasks.Remove(task);

    project.AddTask(task);
    Console.WriteLine($"Assigned task #{task.Id} to project '{project.Name}'.");
}


void FilterTasks()
{
    Console.WriteLine("Status (Todo, InProgress, Done; gol = oricare):");
    string statusInput = Console.ReadLine()?.Trim() ?? "";

    TaskItemStatus? status = null;

    if (!string.IsNullOrWhiteSpace(statusInput))
    {
        if (!Enum.TryParse(statusInput, true, out TaskItemStatus parsedStatus)
            || !Enum.IsDefined(parsedStatus))
        {
            Console.WriteLine("Invalid status.");
            return;
        }

        status = parsedStatus;
    }

    Console.WriteLine("Priority (Low, Medium, High, Critical; gol = oricare):");
    string priorityInput = Console.ReadLine()?.Trim() ?? "";

    TaskPriority? priority = null;

    if (!string.IsNullOrWhiteSpace(priorityInput))
    {
        if (!Enum.TryParse(priorityInput, true, out TaskPriority parsedPriority)
            || !Enum.IsDefined(parsedPriority))
        {
            Console.WriteLine("Invalid priority.");
            return;
        }

        priority = parsedPriority;
    }

    IReadOnlyList<TaskItem> results = taskService.Filter(status, priority);

    PrintTasks(results, "Filtered tasks");
}


void ShowStatistics()
{
    IReadOnlyDictionary<TaskItemStatus, int> counts = taskService.CountByStatus();

    Console.WriteLine("\n--- Statistics ---");
    Console.WriteLine($"Total tasks: {GetTasks().Count}");

    foreach (TaskItemStatus status in Enum.GetValues<TaskItemStatus>())
    {
        counts.TryGetValue(status, out int count);
        Console.WriteLine($"{status}: {count}");
    }

    Console.WriteLine($"Has overdue tasks: {taskService.HasOverdue()}");
}

void SearchTasks()
{
    string query = ConsoleInput.ReadText("Search title: ");
    PrintTasks(taskService.Search(query), $"Search: {query}");
}

void DeleteTask()
{
    TaskItem? task = AskForTask();

    if (task is null)
    {
        Console.WriteLine("Task not found.");
        return;
    }

    Console.WriteLine(taskService.Delete(task.Id)
        ? $"Deleted task #{task.Id}."
        : "Could not delete task.");
}

void ExportTasks()
{
    string exportDirectory = Path.Combine(dataDirectory, "export");
    Directory.CreateDirectory(exportDirectory);

    string exportPath = Path.Combine(
        exportDirectory, "tasks-export.json");

    var exporter = new TaskExporter();
    string json = exporter.Export(taskService.GetAll());

    File.WriteAllText(exportPath, json);

    Console.WriteLine($"Export saved to: {exportPath}");
}

