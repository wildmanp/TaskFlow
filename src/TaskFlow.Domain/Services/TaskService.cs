
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly INotifier _notifier;
    private readonly TimeProvider _timeProvider;

    public TaskService(
        ITaskRepository repository,
        INotifier notifier,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _notifier = notifier;
        _timeProvider = timeProvider;
    }

    public TaskItem Create(
        string title,
        TaskPriority priority,
        DateTime? dueDate,
        string? description = null)
    {
        DateTime now = _timeProvider.GetLocalNow().DateTime;

        if (dueDate.HasValue && dueDate.Value.Date < now.Date)
        {
            throw new ArgumentException(
                "Due date cannot be in the past.", nameof(dueDate));
        }

        TaskItem task = new(title, priority, dueDate, description);

        _repository.Add(task);
        _notifier.Notify($"Task created: {task.Title} (ID: {task.Id})");

        return task;
    }

    public IReadOnlyList<TaskItem> GetAll() => _repository.GetAll();

    public IReadOnlyList<TaskItem> Search(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return _repository.GetAll()
            .Where(task => task.Title.Contains(
                text, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void Start(int id)
    {
        TaskItem task = GetTaskOrThrow(id);

        int inProgressCount = _repository.GetAll()
            .Count(item => item.Status == TaskItemStatus.InProgress);

        if (task.Status != TaskItemStatus.InProgress &&
            inProgressCount >= 3)
        {
            throw new InvalidOperationException(
                "You cannot have more than 3 tasks in progress.");
        }

        task.Start();
        _repository.Update(task);
        _notifier.Notify($"Task started: {task.Title} (ID: {task.Id})");
    }

    public void Complete(int id)
    {
        TaskItem task = GetTaskOrThrow(id);
        task.Complete(_timeProvider.GetLocalNow().DateTime);
        _repository.Update(task);
        _notifier.Notify($"Task completed: {task.Title} (ID: {task.Id})");
    }

    public void Reopen(int id)
    {
        TaskItem task = GetTaskOrThrow(id);
        task.Reopen();
        _repository.Update(task);
        _notifier.Notify($"Task reopened: {task.Title} (ID: {task.Id})");
    }

    public bool Delete(int id)
    {
        TaskItem? task = _repository.GetById(id);

        if (task is null || !_repository.Delete(id))
            return false;

        _notifier.Notify($"Task deleted: {task.Title} (ID: {task.Id})");
        return true;
    }

    public void Update(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        _repository.Update(task);
        _notifier.Notify(
            $"Task updated: {task.Title} (ID: {task.Id})");
    }

    public bool HasOverdue()
    {
        DateTime now = _timeProvider.GetLocalNow().DateTime;
        return _repository.GetAll().Any(task => task.IsOverdue(now));
    }

    public IReadOnlyDictionary<TaskItemStatus, int> CountByStatus()
    {
        return _repository.GetAll()
            .GroupBy(task => task.Status)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    private TaskItem GetTaskOrThrow(int id)
    {
        return _repository.GetById(id)
            ?? throw new KeyNotFoundException(
                $"Task with ID {id} was not found.");
    }
    public IReadOnlyList<TaskItem> Filter(
    TaskItemStatus? status,
    TaskPriority? priority)
{
    return _repository.GetAll()
        .Where(t =>
            (status is null || t.Status == status) &&
            (priority is null || t.Priority == priority))
        .ToList();
}
}
