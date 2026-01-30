using Microsoft.Extensions.Logging;

namespace GameStore.Shared.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private bool _disposed;

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
        }

        _disposed = true;
    }
}
