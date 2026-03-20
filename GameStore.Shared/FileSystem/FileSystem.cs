using GameStore.Shared.Exceptions;

namespace GameStore.Shared.FileSystem;

public class FileSystem : IFileSystem
{
    public bool FileExists(string path)
        => File.Exists(path);

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public Task WriteFileAsync(string path, byte[] content)
    {
        return string.IsNullOrEmpty(path)
            ? throw new BadRequestException("Path cannot be null or empty.")
            : content == null || content.Length == 0
            ? throw new BadRequestException("The file content can`t be empty.")
            : WriteFileInternalAsync(path, content);
    }

    public async Task<byte[]> ReadFileAsync(string path)
    {
        return await File.ReadAllBytesAsync(path);
    }

    private static async Task WriteFileInternalAsync(string path, byte[] content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllBytesAsync(path, content);
    }
}