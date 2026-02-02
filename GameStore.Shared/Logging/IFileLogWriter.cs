using Microsoft.Extensions.Logging;

namespace GameStore.Shared.Logging;

public interface IFileLogWriter
{
    void WriteLog<TState>(
       LogLevel logLevel,
       TState state);

    void WriteExceptions<TState>(
       LogLevel logLevel,
       TState state,
       Exception? exception,
       Func<TState, Exception?, string> formatter);
}
