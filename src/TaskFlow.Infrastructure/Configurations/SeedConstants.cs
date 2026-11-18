
namespace TaskFlow.Infrastructure.Configurations;

internal static class SeedConstants
{
    // Doar constante: valoarea din HasData ajunge în codul migrării
    public static readonly DateTime SeedDate = new(
        2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
