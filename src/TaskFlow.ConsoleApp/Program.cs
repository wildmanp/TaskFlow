
using TaskFlow.ConsoleApp;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

var tasks = new List<TaskItem>();
var projects = new Dictionary<int, Project>();
var nextId = 1;
var nextProjectId = 1;

AddTask(new TaskItem("Install .NET 10 SDK", TaskPriority.High, DateTime.Today.AddDays(-2)));
AddTask(new TaskItem("Read about value and reference types", TaskPriority.Medium, DateTime.Today.AddDays(3)));
AddTask(new TaskItem("Push the first commit", TaskPriority.Low));

while (true)
{
    Console.WriteLine("\nTaskFlow console, week 01");
    Console.WriteLine("1) Add task");
    Console.WriteLine("2) List tasks");
    Console.WriteLine("3) Start task");
    Console.WriteLine("4) Complete task");
    Console.WriteLine("5) Show overdue tasks");
    Console.WriteLine("6) Reopen task");
    Console.WriteLine("7) Show task details");
    Console.WriteLine("8) Edit task");
    Console.WriteLine("9) Create project");
    Console.WriteLine("10) List projects");
    Console.WriteLine("11) Assign task to project");
    Console.WriteLine("12) Filter tasks by priority");
    Console.WriteLine("13) Filter tasks by status");
    Console.WriteLine("14) Show statistics");
    Console.WriteLine("0) Exit");

    var choice = ConsoleInput.ReadText("Choose an option: ").Trim();

    try
    {
        switch (choice)
        {
            case "1":
                AddFromInput();
                break;
            case "2":
                PrintTasks(tasks, "All tasks");
                break;
            case "3":
                StartTask();
                break;
            case "4":
                CompleteTask();
                break;
            case "5":
                PrintTasks(FindOverdue(DateTime.Now), "Overdue tasks");
                break;
            case "6":
                ReopenTask();
                break;
            case "7":
                ShowTaskDetails();
                break;
            case "8":
                EditTask();
                break;
            case "9":
                CreateProject();
                break;
            case "10":
                ListProjects();
                break;
            case "11":
                AssignTaskToProject();
                break;
            case "12":
                FilterByPriority();
                break;
            case "13":
                FilterByStatus();
                break;
            case "14":
                ShowStatistics();
                break;
            case "0":
                Console.WriteLine("Bye!");
                return;
            default:
                Console.WriteLine($"Unknown option '{choice}'.");
                break;
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Not allowed: {ex.Message}");
    }
}

void AddTask(TaskItem task)
{
    task.Id = nextId++;
    tasks.Add(task);
}

void AddFromInput()
{
    var title = ConsoleInput.ReadText("Title: ");
    var priority = ConsoleInput.ReadPriority();
    var dueDate = ConsoleInput.ReadOptionalDate(
        "Due date (yyyy-MM-dd, empty = none): ");
    var description = ConsoleInput.ReadText("Description (optional): ");

    var task = new TaskItem(title, priority, dueDate, description);
    AddTask(task);

    Console.WriteLine($"Added: {task}");
}

TaskItem? AskForTask()
{
    var id = ConsoleInput.ReadInt("Task id: ");
    var task = tasks.FirstOrDefault(t => t.Id == id);

    if (task is null)
        Console.WriteLine($"Task #{id} not found.");

    return task;
}

void StartTask()
{
    var task = AskForTask();

    if (task is null)
        return;

    task.Start();
    Console.WriteLine($"Started: {task}");
}

void CompleteTask()
{
    var task = AskForTask();

    if (task is null)
        return;

    task.Complete(DateTime.Now);
    Console.WriteLine($"Completed: {task}");
}

void ReopenTask()
{
    var task = AskForTask();

    if (task is null)
        return;

    task.Reopen();
    Console.WriteLine($"Reopened: {task}");
}

void ShowTaskDetails()
{
    var task = AskForTask();

    if (task is null)
        return;

    Console.WriteLine($"Id: {task.Id}");
    Console.WriteLine($"Title: {task.Title}");
    Console.WriteLine($"Description: {task.Description ?? "-"}");
    Console.WriteLine($"Priority: {task.Priority}");
    Console.WriteLine($"Status: {task.Status}");
    Console.WriteLine($"DueDate: {task.DueDate?.ToString("yyyy-MM-dd") ?? "-"}");
    Console.WriteLine($"CreatedAt: {task.CreatedAt:yyyy-MM-dd HH:mm}");
    Console.WriteLine($"CompletedAt: {task.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-"}");
    Console.WriteLine($"Project: {task.Project?.Name ?? "-"}");
}

void EditTask()
{
    var task = AskForTask();

    if (task is null)
        return;

    var title = ConsoleInput.ReadText($"Title [{task.Title}] (empty = keep): ");
    var description = ConsoleInput.ReadText(
        $"Description [{task.Description ?? "-"}] (empty = keep): ");
    var priority = ConsoleInput.ReadOptionalPriority();
    var dueDateInput = ConsoleInput.ReadText(
        $"Due date [{task.DueDate?.ToString("yyyy-MM-dd") ?? "-"}] (empty = keep): ");

    try
    {
        if (!string.IsNullOrWhiteSpace(title))
            task.Title = title;

        if (!string.IsNullOrWhiteSpace(description))
            task.Description = description;

        if (priority.HasValue)
            task.Priority = priority.Value;

        if (!string.IsNullOrWhiteSpace(dueDateInput))
        {
            if (!DateTime.TryParseExact(
                    dueDateInput.Trim(),
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var dueDate))
            {
                Console.WriteLine("Validation error: Use yyyy-MM-dd.");
                return;
            }

            task.DueDate = dueDate;
        }

        Console.WriteLine($"Updated: {task}");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
}

void CreateProject()
{
    var name = ConsoleInput.ReadText("Project name: ").Trim();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Validation error: Project name is required.");
        return;
    }

    foreach (var existingProject in projects.Values)
    {
        if (string.Equals(
                existingProject.Name,
                name,
                StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("A project with this name already exists.");
            return;
        }
    }

    var description = ConsoleInput.ReadText("Description (optional): ");

    try
    {
        var project = new Project(name, description)
        {
            Id = nextProjectId++
        };

        projects.Add(project.Id, project);
        Console.WriteLine($"Created project #{project.Id}: {project.Name}");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
}

void ListProjects()
{
    if (projects.Count == 0)
    {
        Console.WriteLine("No projects found.");
        return;
    }

    foreach (var project in projects.Values)
    {
        Console.WriteLine(
            $"#{project.Id} {project.Name} (tasks: {project.Tasks.Count}, overdue: {project.CountOverdue(DateTime.Now)})");
    }
}

void AssignTaskToProject()
{
    var task = AskForTask();

    if (task is null)
        return;

    var projectId = ConsoleInput.ReadInt("Project id: ");

    if (!projects.TryGetValue(projectId, out var project))
    {
        Console.WriteLine($"Project #{projectId} not found.");
        return;
    }

    if (task.ProjectId.HasValue)
    {
        Console.WriteLine("This task is already assigned to a project.");
        return;
    }

    project.AddTask(task);
    Console.WriteLine($"Assigned task #{task.Id} to project {project.Name}.");
}

List<TaskItem> FindOverdue(DateTime now)
{
    var result = new List<TaskItem>();

    foreach (var task in tasks)
    {
        if (task.IsOverdue(now))
            result.Add(task);
    }

    return result;
}

void FilterByPriority()
{
    var priority = ConsoleInput.ReadPriority();
    var result = new List<TaskItem>();

    foreach (var task in tasks)
    {
        if (task.Priority == priority)
            result.Add(task);
    }

    PrintTasks(result, $"Tasks with priority {priority}");
}

void FilterByStatus()
{
    var status = ConsoleInput.ReadStatus();
    var result = new List<TaskItem>();

    foreach (var task in tasks)
    {
        if (task.Status == status)
            result.Add(task);
    }

    PrintTasks(result, $"Tasks with status {status}");
}

void ShowStatistics()
{
    var counts = new Dictionary<TaskItemStatus, int>();

    foreach (var status in Enum.GetValues<TaskItemStatus>())
        counts[status] = 0;

    foreach (var task in tasks)
        counts[task.Status]++;

    Console.WriteLine("--- Task statistics ---");

    foreach (var status in Enum.GetValues<TaskItemStatus>())
        Console.WriteLine($"{status}: {counts[status]}");

    Console.WriteLine($"Overdue: {FindOverdue(DateTime.Now).Count}");
}

void PrintTasks(List<TaskItem> items, string heading)
{
    Console.WriteLine($"--- {heading}: {items.Count} ---");

    if (items.Count == 0)
    {
        Console.WriteLine("(empty)");
        return;
    }

    foreach (var task in items)
        Console.WriteLine(task);
}
