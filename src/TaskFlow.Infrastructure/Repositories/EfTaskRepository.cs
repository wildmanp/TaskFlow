
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class EfTaskRepository(TaskFlowDbContext db) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> SearchAsync(
        TaskFilter filter,
        CancellationToken ct = default)
    {
        IQueryable<TaskItem> query = db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Tags);

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
                t.Title.Contains(search) ||
                (t.Description != null &&
                 t.Description.Contains(search)));
        }

        return await query
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<TaskItem?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return await db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task AddAsync(
        TaskItem task,
        CancellationToken ct = default)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(
        TaskItem task,
        CancellationToken ct = default)
    {
        if (db.Entry(task).State == EntityState.Detached)
            db.Tasks.Update(task);

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync(new object[] { id }, ct);

        if (task is null)
            return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<TaskStatistics> GetStatisticsAsync(
        DateTime now,
        CancellationToken ct = default)
    {
        var statusQuery = db.Tasks
            .AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            });

        var priorityQuery = db.Tasks
            .AsNoTracking()
            .GroupBy(t => t.Priority)
            .Select(g => new
            {
                Priority = g.Key,
                Count = g.Count()
            });

        var statusRows = await statusQuery.ToListAsync(ct);
        var priorityRows = await priorityQuery.ToListAsync(ct);

        var overdueTasks = await db.Tasks
            .AsNoTracking()
            .CountAsync(
                t => t.Status != TaskItemStatus.Done
                     && t.DueDate.HasValue
                     && t.DueDate.Value.Date < now.Date,
                ct);

        var totalTasks = statusRows.Sum(x => x.Count);

        return new TaskStatistics(
            TotalTasks: totalTasks,
            TasksByStatus: statusRows.ToDictionary(
                x => x.Status,
                x => x.Count),
            TasksByPriority: priorityRows.ToDictionary(
                x => x.Priority,
                x => x.Count),
            OverdueTasks: overdueTasks);
    }

    public async Task<bool> ReplaceTagsAsync(
        int taskId,
        IReadOnlyCollection<int> tagIds,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tagIds);

        var task = await db.Tasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task is null)
            return false;

        var uniqueTagIds = tagIds.Distinct().ToArray();

        var tags = await db.Tags
            .Where(t => uniqueTagIds.Contains(t.Id))
            .ToListAsync(ct);

        if (tags.Count != uniqueTagIds.Length)
            throw new ArgumentException(
                "One or more tag IDs do not exist.",
                nameof(tagIds));

        task.Tags.Clear();

        foreach (var tag in tags)
            task.Tags.Add(tag);

        await db.SaveChangesAsync(ct);

        return true;
    }
}
