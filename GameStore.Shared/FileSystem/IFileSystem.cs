namespace GameStore.Shared.FileSystem;

public interface IFileSystem
{
    bool FileExists(string path);
}