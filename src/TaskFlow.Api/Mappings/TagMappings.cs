
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Mappings;

public static class TagMappings
{
    public static TagDto ToDto(this TagSummary tag)
    {
        return new TagDto(
            tag.Id,
            tag.Name,
            tag.TaskCount);
    }

    public static Tag ToEntity(this CreateTagRequest request)
    {
        return new Tag(request.Name);
    }
}
