namespace GameStore.Shared.Exceptions;

public class GameKeyNotUniqueException(string gameKey) : Exception($"Game key '{gameKey}' is not unique.")
{
}
