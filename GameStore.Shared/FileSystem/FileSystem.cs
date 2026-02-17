namespace GameStore.Shared.FileSystem;

public class FileSystem : IFileSystem
{
    public bool FileExists(string path)
        => File.Exists(path);

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public async Task WriteFileAsync(string path, byte[] content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllBytesAsync(path, content);
    }

    public async Task<byte[]> ReadFileAsync(string path)
    {
        return await File.ReadAllBytesAsync(path);
    }
}