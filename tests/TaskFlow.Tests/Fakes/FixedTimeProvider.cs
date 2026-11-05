namespace TaskFlow.Tests.Fakes;

public sealed class FixedTimeProvider(DateTime fixedTime) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() =>
        new(DateTime.SpecifyKind(fixedTime, DateTimeKind.Utc));
}