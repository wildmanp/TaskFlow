using TaskFlow.ConsoleApp;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

var tasks = new List<TaskItem>();
var nextId = 1;

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

    var task = new TaskItem(title, priority, dueDate);
    AddTask(task);

    Console.WriteLine($"Added: {task}");
}

void StartTask()
{
    var id = ConsoleInput.ReadInt("Task id: ");
    var task = tasks.FirstOrDefault(t => t.Id == id);

    if (task is null)
    {
        Console.WriteLine($"Task #{id} not found.");
        return;
    }

    task.Start();
    Console.WriteLine($"Started: {task}");
}

void CompleteTask()
{
    var id = ConsoleInput.ReadInt("Task id: ");
    var task = tasks.FirstOrDefault(t => t.Id == id);

    if (task is null)
    {
        Console.WriteLine($"Task #{id} not found.");
        return;
    }

    task.Complete(DateTime.Now);
    Console.WriteLine($"Completed: {task}");
}

List<TaskItem> FindOverdue(DateTime now)
{
    return tasks.Where(t => t.IsOverdue(now)).ToList();
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

