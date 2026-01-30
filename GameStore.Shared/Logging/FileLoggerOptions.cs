namespace GameStore.Shared.Logging;

public static class FileLoggerOptions
{
    public static readonly string LogDirectory = "Logs";

    public static readonly string FileLogs = "app";

    public static readonly string FileExceptions = "exceptions";

    public static readonly string DateFormat = "yyyy-MM-dd";

    public static readonly string FileExtension = ".log";
}
