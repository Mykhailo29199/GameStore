namespace GameStore.Shared.Exceptions;

public class Exceptions
{
    public class GameKeyNotUniqueException(string gameKey) : Exception($"Game key '{gameKey}' is not unique.")
    {
    }

    public class GameNoNameException : Exception
    {
        public GameNoNameException()
            : base("Game name is null or empty")
        {
        }
    }

    public class PlatformNotFoundException(Guid platformId) : Exception($"Platform with ID '{platformId}' is not found.")
    {
    }

    public class GenreNotFoundException(Guid genreId) : Exception($"Genre with ID '{genreId}' is not found.")
    {
    }
}