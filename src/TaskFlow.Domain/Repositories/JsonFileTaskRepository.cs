
using System.Text.Json;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Serialization;

namespace TaskFlow.Domain.Repositories;

public class JsonFileTaskRepository : ITaskRepository
{
    private readonly string _filePath;
    private readonly List<TaskItem> _tasks;
    private int _nextId;

    public JsonFileTaskRepository(string filePath)
    {
        _filePath = filePath;

        string? directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);

            _tasks = JsonSerializer.Deserialize<List<TaskItem>>(
                json, JsonOptions.Default) ?? new List<TaskItem>();
        }
        else
        {
            _tasks = new List<TaskItem>();
        }

        _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(task => task.Id) + 1;
    }

    public IReadOnlyList<TaskItem> GetAll() => _tasks.AsReadOnly();

    public TaskItem? GetById(int id) =>
        _tasks.FirstOrDefault(task => task.Id == id);

    public void Add(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (_tasks.Any(existing => existing.Id == task.Id && task.Id != 0))
        {
            throw new InvalidOperationException(
                $"Task with ID {task.Id} already exists.");
        }

        if (task.Id == 0)
        {
            task.Id = _nextId++;
        }

        _tasks.Add(task);
        Save();
    }

    public void Update(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        int index = _tasks.FindIndex(existing => existing.Id == task.Id);

        if (index < 0)
        {
            throw new InvalidOperationException(
                $"Task with ID {task.Id} was not found.");
        }

        _tasks[index] = task;
        Save();
    }

   public bool Delete(int id)
{
    TaskItem? task = GetById(id);

    if (task is null)
        return false;

    _tasks.Remove(task);
    Save();
    return true;
}

    private void Save()
    {
        string json = JsonSerializer.Serialize(_tasks, JsonOptions.Default);
        File.WriteAllText(_filePath, json);
    }
}
