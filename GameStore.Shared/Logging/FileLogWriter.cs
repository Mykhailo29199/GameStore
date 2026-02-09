using System.Text;
using Microsoft.Extensions.Logging;
using static GameStore.Shared.Logging.FileLoggerOptions;

namespace GameStore.Shared.Logging;

public class FileLogWriter : IFileLogWriter
{
    private readonly FileSystem.IFileSystem _fileSystem;
    private static readonly object _lockObject = new();

    public FileLogWriter(FileSystem.IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _fileSystem.CreateDirectory(LogDirectory);
    }

    public void WriteLog<TState>(LogLevel logLevel, TState state)
    {
        var filePath = GetLogFilePath("app");
        var sb = new StringBuilder();

        sb.AppendLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {logLevel}");
        sb.AppendLine(state?.ToString());
        sb.AppendLine(new string('-', 80));

        lock (_lockObject)
        {
            File.AppendAllText(filePath, sb.ToString());
        }
    }

    public void WriteExceptions<TState>(
        LogLevel logLevel,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var filePath = GetLogFilePath("exceptions");
        var sb = new StringBuilder();

        sb.AppendLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {logLevel}");
        sb.AppendLine(formatter(state, exception));

        if (exception != null)
        {
            sb.AppendLine("Exception Details:");
            sb.AppendLine(exception.ToString());
        }

        sb.AppendLine(new string('-', 80));

        lock (_lockObject)
        {
            File.AppendAllText(filePath, sb.ToString());
        }
    }

    private static string GetLogFilePath(string type)
    {
        var date = DateTime.UtcNow.ToString(DateFormat);
        var fileName = $"{type}-{date}{FileExtension}";

        return Path.Combine(LogDirectory, fileName);
    }
}
