
using TaskFlow.Domain.Abstractions;

namespace TaskFlow.Domain.Notifications;

public class ConsoleNotifier : INotifier
{
    public void Notify(string message)
    {
        Console.WriteLine($"[NOTIFICATION] {message}");
    }
}

