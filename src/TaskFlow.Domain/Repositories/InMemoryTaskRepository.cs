
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();
    private int _nextId = 1;

    public IReadOnlyList<TaskItem> GetAll()
    {
        return _tasks.AsReadOnly();
    }

    public TaskItem? GetById(int id)
    {
        return _tasks.FirstOrDefault(task => task.Id == id);
    }

    public void Add(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.Id == 0)
        {
            task.Id = _nextId++;
        }

        _tasks.Add(task);
    }

    public void Update(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        int index = _tasks.FindIndex(existing => existing.Id == task.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Task with ID {task.Id} was not found.");
        }

        _tasks[index] = task;
    }

    public bool Delete(int id)
{
    TaskItem? task = GetById(id);

    if (task is null)
        return false;

    _tasks.Remove(task);
    return true;
}
}
