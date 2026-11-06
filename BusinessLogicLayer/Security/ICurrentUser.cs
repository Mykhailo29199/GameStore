namespace BusinessLogicLayer.Security;

/// <summary>
/// The user that performs the current request. A guest (not authorized) has no id and no permissions.
/// </summary>
public interface ICurrentUser
{
    Guid? Id { get; }

    string? Name { get; }

    bool IsAuthenticated { get; }

    bool HasPermission(string permission);
}
