
using TaskFlow.Domain.Abstractions;

namespace TaskFlow.Domain.Notifications;

public class CompositeNotifier : INotifier
{
    private readonly IReadOnlyList<INotifier> _notifiers;

    public CompositeNotifier(params INotifier[] notifiers)
    {
        ArgumentNullException.ThrowIfNull(notifiers);
        _notifiers = notifiers;
    }

    public void Notify(string message)
    {
        foreach (INotifier notifier in _notifiers)
        {
            notifier.Notify(message);
        }
    }
}
