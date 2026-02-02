namespace GameStore.Shared.FileSystem;

public class FileSystem : IFileSystem
{
    public bool FileExists(string path)
        => File.Exists(path);

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}