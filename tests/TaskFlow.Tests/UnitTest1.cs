
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;
using TaskFlow.Domain.Reports;

namespace TaskFlow.Tests;

public class UnitTest1
{
    private sealed class FakeNotifier : INotifier
    {
        public List<string> Messages { get; } = new();

        public void Notify(string message) => Messages.Add(message);
    }

    private readonly InMemoryTaskRepository _repository = new();
    private readonly FakeNotifier _notifier = new();

    private TaskService CreateService() =>
        new(_repository, _notifier, TimeProvider.System);

    [Fact]
    public void Create_SavesTaskAndSendsNotification()
    {
        // Arrange
        var service = CreateService();

        // Act
        var task = service.Create("Learn C#", TaskPriority.High, null);

        // Assert
        Assert.True(task.Id > 0);
        Assert.Single(service.GetAll());
        Assert.Contains(_notifier.Messages, m => m.Contains("Task created"));
    }

    [Fact]
    public void Search_IsCaseInsensitive()
    {
        // Arrange
        var service = CreateService();
        service.Create("Learn CSharp", TaskPriority.Medium, null);
        service.Create("Buy groceries", TaskPriority.Low, null);

        // Act
        var results = service.Search("csharp");

        // Assert
        Assert.Single(results);
        Assert.Equal("Learn CSharp", results[0].Title);
    }

    [Fact]
    public void Complete_SetsStatusToDone()
    {
        // Arrange
        var service = CreateService();
        var task = service.Create("Finish project", TaskPriority.High, null);

        // Act
        service.Complete(task.Id);

        // Assert
        Assert.Equal(TaskItemStatus.Done, task.Status);
        Assert.Contains(_notifier.Messages, m => m.Contains("Task completed"));
    }

    [Fact]
    public void Delete_RemovesTask()
    {
        // Arrange
        var service = CreateService();
        var task = service.Create("Delete me", TaskPriority.Low, null);

        // Act
        bool result = service.Delete(task.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void Start_RejectsFourthInProgressTask()
    {
        // Arrange
        var service = CreateService();
        var first = service.Create("Task 1", TaskPriority.Low, null);
        var second = service.Create("Task 2", TaskPriority.Low, null);
        var third = service.Create("Task 3", TaskPriority.Low, null);
        var fourth = service.Create("Task 4", TaskPriority.Low, null);

        service.Start(first.Id);
        service.Start(second.Id);
        service.Start(third.Id);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => service.Start(fourth.Id));
    }

    [Theory]
    [InlineData(-1, false, true)]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(-1, true, false)]
    public void IsOverdue_ReturnsExpectedResult(
        int daysFromToday, bool isDone, bool expected)
    {
        // Arrange
        DateTime today = new(2026, 11, 1);
        var dueDate = today.AddDays(daysFromToday);
        var task = new TaskItem(
            "Test task", TaskPriority.Medium, dueDate);

        if (isDone)
            task.Complete(today);

        // Act
        bool result = task.IsOverdue(today);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Reopen_TodoTask_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var task = service.Create("Todo task", TaskPriority.Low, null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => service.Reopen(task.Id));
    }

    [Fact]
    public void Search_FindsDemoCaseInsensitively()
    {
        // Arrange
        var service = CreateService();
        service.Create("Pregătește Demo", TaskPriority.High, null);

        // Act
        var results = service.Search("demo");

        // Assert
        Assert.Single(results);
        Assert.Equal("Pregătește Demo", results[0].Title);
    }

    [Fact]
    public void Delete_NonexistentId_ReturnsFalseWithoutNotification()
    {
        // Arrange
        var service = CreateService();
        int messagesBefore = _notifier.Messages.Count;

        // Act
        bool result = service.Delete(999);

        // Assert
        Assert.False(result);
        Assert.Equal(messagesBefore, _notifier.Messages.Count);
    }

    [Fact]
    public void InMemoryRepository_Add_AssignsConsecutiveIds()
    {
        // Arrange
        var repository = new InMemoryTaskRepository();
        var first = new TaskItem("First", TaskPriority.Low, null);
        var second = new TaskItem("Second", TaskPriority.Low, null);
        var third = new TaskItem("Third", TaskPriority.Low, null);

        // Act
        repository.Add(first);
        repository.Add(second);
        repository.Add(third);

        // Assert
        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
        Assert.Equal(3, third.Id);
    }

    [Fact]
    public void DueSoonReport_IncludesTaskDueInThreeDays_ExcludesTenDays()
    {
        // Arrange
        DateTime today = new(2026, 11, 1);
        var soon = new TaskItem(
            "Soon task", TaskPriority.High, today.AddDays(3));
        var later = new TaskItem(
            "Later task", TaskPriority.Medium, today.AddDays(10));

        // Act
        var report = DueSoonReport.Generate(
            new[] { soon, later }, today);

        // Assert
        Assert.Contains(report.Tasks, t => t.Title == "Soon task");
        Assert.DoesNotContain(report.Tasks, t => t.Title == "Later task");
    }

    [Fact]
    public void Filter_TodoAndAnyPriority_ReturnsOnlyTodoTasks()
    {
        // Arrange
        var service = CreateService();
        var todo = service.Create("Todo", TaskPriority.Low, null);
        var inProgress = service.Create(
            "In progress", TaskPriority.High, null);
        service.Start(inProgress.Id);

        // Act
        var results = service.Filter(TaskItemStatus.Todo, null);

        // Assert
        Assert.Single(results);
        Assert.Equal(todo.Id, results[0].Id);
        Assert.All(results,
            t => Assert.Equal(TaskItemStatus.Todo, t.Status));
    }
        [Fact]
    public void Filter_ByPriority_ReturnsOnlyMatchingTasks()
    {
        // Arrange
        var service = CreateService();
        service.Create("High task", TaskPriority.High, null);
        service.Create("Low task", TaskPriority.Low, null);

        // Act
        var results = service.Filter(null, TaskPriority.High);

        // Assert
        Assert.Single(results);
        Assert.Equal("High task", results[0].Title);
        Assert.All(results,
            t => Assert.Equal(TaskPriority.High, t.Priority));
    }
}
