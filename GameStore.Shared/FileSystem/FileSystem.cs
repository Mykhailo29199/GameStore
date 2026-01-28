namespace GameStore.Shared.FileSystem;

public class FileSystem : IFileSystem
{
    public bool FileExists(string path)
        => File.Exists(path);
}