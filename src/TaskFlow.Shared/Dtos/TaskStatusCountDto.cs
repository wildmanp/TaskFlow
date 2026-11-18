namespace TaskFlow.Shared.Dtos;

public sealed record TaskStatusCountDto(
    string Status,
    int Count);