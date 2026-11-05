using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;
using TaskFlow.Tests.Fakes;

namespace TaskFlow.Tests;

public class ProjectServiceTests
{
    private readonly InMemoryProjectRepository _projectRepository = new();
    private readonly InMemoryTaskRepository _taskRepository = new();

    private ProjectService CreateService() => new(
        _projectRepository,
        _taskRepository,
        new FixedTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0)));

    [Fact]
    public async Task CreateAsync_AssignsId()
    {
        var service = CreateService();

        var created = await service.CreateAsync(new Project("TaskFlow"));

        Assert.Equal(1, created.Id);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameIgnoringCase_Throws()
    {
        var service = CreateService();

        await service.CreateAsync(new Project("TaskFlow"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(new Project("taskflow")));
    }

    [Fact]
    public async Task UpdateAsync_RenamesProject()
    {
        var service = CreateService();

        var project = await service.CreateAsync(new Project("TaskFlow"));
        project.Name = "TaskFlow Updated";

        await service.UpdateAsync(project);

        var updated = await service.GetByIdAsync(project.Id);

        Assert.Equal("TaskFlow Updated", updated!.Name);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_Throws()
    {
        var service = CreateService();

        await service.CreateAsync(new Project("TaskFlow"));
        var second = await service.CreateAsync(new Project("Website"));

        second.Name = "TaskFlow";

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(second));
    }

    [Fact]
    public async Task DeleteAsync_ProjectWithTasks_Throws()
    {
        var service = CreateService();

        var project = await service.CreateAsync(new Project("TaskFlow"));

        var task = new TaskItem("Create homepage")
        {
            ProjectId = project.Id
        };

        await _taskRepository.AddAsync(task);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteAsync(project.Id));
    }
}