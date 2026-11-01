
using TaskFlow.Domain.Abstractions;

namespace TaskFlow.Domain.Notifications;

public class FileNotifier : INotifier
{
    private readonly string _filePath;

    public FileNotifier(string filePath)
    {
        _filePath = filePath;

        string? directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void Notify(string message)
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        File.AppendAllText(_filePath, entry + Environment.NewLine);
    }
}
