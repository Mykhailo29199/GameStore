using Microsoft.Extensions.Logging;

namespace GameStore.Shared.Logging;

public class FileLogger() : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
     where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var fileLogWriter = new FileLogWriter(new FileSystem.FileSystem());

        if (logLevel == LogLevel.Error)
        {
            fileLogWriter.WriteExceptions(logLevel, state, exception, formatter);
        }
        else
        {
            fileLogWriter.WriteLog(logLevel, state);
        }
    }
}
