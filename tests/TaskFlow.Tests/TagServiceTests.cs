using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;

namespace TaskFlow.Tests;

public class TagServiceTests
{
    private readonly InMemoryTagRepository _repository = new();
    private readonly TagService _service;

    public TagServiceTests()
    {
        _service = new TagService(_repository);
    }

    [Fact]
    public async Task CreateAsync_AssignsId()
    {
        var created = await _service.CreateAsync(new Tag("Backend"));

        Assert.Equal(1, created.Id);
        Assert.Equal("Backend", created.Name);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameIgnoringCase_Throws()
    {
        await _service.CreateAsync(new Tag("Backend"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(new Tag("backend")));
    }
}