using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Tests;

public class JsonFileTaskRepositoryTests
{
    [Fact]
    public async Task Add_ThenNewInstance_ReadsSameTasksFromFile()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");

        try
        {
            var first = new JsonFileTaskRepository(filePath);
            var task = new TaskItem("Persist me");

            await first.AddAsync(task);

            var second = new JsonFileTaskRepository(filePath);

            var savedTask = Assert.Single(await second.GetAllAsync());

            Assert.Equal("Persist me", savedTask.Title);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}