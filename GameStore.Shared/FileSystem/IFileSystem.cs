namespace GameStore.Shared.FileSystem;

public interface IFileSystem
{
    bool FileExists(string path);

    void CreateDirectory(string path);

    Task WriteFileAsync(string path, byte[] content);

    Task<byte[]> ReadFileAsync(string path);
}