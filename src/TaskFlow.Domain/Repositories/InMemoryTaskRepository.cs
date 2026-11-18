
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();
    private readonly Lock _sync = new();
    private int _lastId;

    public InMemoryTaskRepository()
    {
    }

    public InMemoryTaskRepository(IEnumerable<TaskItem> existingTasks)
    {
        _tasks.AddRange(existingTasks);
        _lastId = _tasks.Count == 0 ? 0 : _tasks.Max(t => t.Id);
    }

    public Task<IReadOnlyList<TaskItem>> GetAllAsync(
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IReadOnlyList<TaskItem> snapshot = _tasks.ToList();
            return Task.FromResult(snapshot);
        }
    }

    public Task<IReadOnlyList<TaskItem>> SearchAsync(
        TaskFilter filter,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IEnumerable<TaskItem> query = _tasks;

            if (filter.Status is not null)
                query = query.Where(t => t.Status == filter.Status);

            if (filter.Priority is not null)
                query = query.Where(t => t.Priority == filter.Priority);

            if (filter.ProjectId is not null)
                query = query.Where(t => t.ProjectId == filter.ProjectId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(t =>
                    t.Title.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null &&
                     t.Description.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)));
            }

            IReadOnlyList<TaskItem> result = query
                .OrderBy(t => t.Id)
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<TaskItem?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            TaskItem? task = _tasks.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(task);
        }
    }

    public Task AddAsync(
        TaskItem task,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ct.ThrowIfCancellationRequested();

        if (task.Id != 0)
            throw new InvalidOperationException(
                "O sarcină nouă nu trebuie să aibă Id: acesta este atribuit de repository.");

        lock (_sync)
        {
            _lastId++;
            task.Id = _lastId;
            _tasks.Add(task);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        TaskItem task,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            int index = _tasks.FindIndex(t => t.Id == task.Id);

            if (index < 0)
                throw new KeyNotFoundException(
                    $"Sarcina #{task.Id} nu a fost găsită.");

            _tasks[index] = task;
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            bool removed = _tasks.RemoveAll(t => t.Id == id) > 0;
            return Task.FromResult(removed);
        }
    }

    public Task<bool> ReplaceTagsAsync(
        int taskId,
        IReadOnlyCollection<int> tagIds,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tagIds);
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);

            if (task is null)
                return Task.FromResult(false);

            var uniqueIds = tagIds.Distinct().ToArray();

            var availableTags = _tasks
                .SelectMany(t => t.Tags)
                .DistinctBy(t => t.Id)
                .ToDictionary(t => t.Id);

            if (uniqueIds.Any(id => !availableTags.ContainsKey(id)))
                throw new ArgumentException(
                    "One or more tag IDs do not exist.",
                    nameof(tagIds));

            foreach (var oldTag in task.Tags)
                oldTag.Tasks.Remove(task);

            task.Tags.Clear();

            foreach (var id in uniqueIds)
            {
                var tag = availableTags[id];
                task.Tags.Add(tag);

                if (!tag.Tasks.Contains(task))
                    tag.Tasks.Add(task);
            }

            return Task.FromResult(true);
        }
    }

    public Task<TaskStatistics> GetStatisticsAsync(
        DateTime now,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var tasks = _tasks.ToList();

            var byStatus = tasks
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var byPriority = tasks
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count());

            var statistics = new TaskStatistics(
                TotalTasks: tasks.Count,
                TasksByStatus: byStatus,
                TasksByPriority: byPriority,
                OverdueTasks: tasks.Count(t => t.IsOverdue(now)));

            return Task.FromResult(statistics);
        }
    }
}
