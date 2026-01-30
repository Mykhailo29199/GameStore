using Microsoft.Extensions.Logging;

namespace GameStore.Shared.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
