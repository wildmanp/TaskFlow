namespace TaskFlow.Shared.Dtos;

public sealed record TaskPriorityCountDto(
    string Priority,
    int Count);