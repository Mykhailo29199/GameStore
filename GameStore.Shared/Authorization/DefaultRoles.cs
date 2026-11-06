namespace GameStore.Shared.Authorization;

/// <summary>
/// Default roles and their permissions. Hierarchy: Admin => Manager => Moderator => User => Guest,
/// every role inherits the permissions of the roles below it.
/// </summary>
public static class DefaultRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Moderator = "Moderator";
    public const string User = "User";
    public const string Guest = "Guest";

    public static readonly Guid AdminId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid ManagerId = Guid.Parse("a0000000-0000-0000-0000-000000000002");
    public static readonly Guid ModeratorId = Guid.Parse("a0000000-0000-0000-0000-000000000003");
    public static readonly Guid UserId = Guid.Parse("a0000000-0000-0000-0000-000000000004");
    public static readonly Guid GuestId = Guid.Parse("a0000000-0000-0000-0000-000000000005");

    private static readonly string[] GuestPermissions = [];

    private static readonly string[] UserPermissions =
    [
        .. GuestPermissions,
        Permissions.BuyGame,
        Permissions.AddComment,
    ];

    private static readonly string[] ModeratorPermissions =
    [
        .. UserPermissions,
        Permissions.DeleteComment,
        Permissions.BanUser,
    ];

    private static readonly string[] ManagerPermissions =
    [
        .. ModeratorPermissions,
        Permissions.AddGame,
        Permissions.UpdateGame,
        Permissions.DeleteGame,
        Permissions.AddGenre,
        Permissions.UpdateGenre,
        Permissions.DeleteGenre,
        Permissions.AddPlatform,
        Permissions.UpdatePlatform,
        Permissions.DeletePlatform,
        Permissions.AddPublisher,
        Permissions.UpdatePublisher,
        Permissions.DeletePublisher,
        Permissions.ViewOrders,
        Permissions.UpdateOrder,
        Permissions.ShipOrder,
        Permissions.ViewOrderHistory,
    ];

    private static readonly string[] AdminPermissions =
    [
        .. ManagerPermissions,
        Permissions.ViewDeletedGames,
        Permissions.UpdateDeletedGame,
        Permissions.ManageDeletedGameComments,
        Permissions.ManageUsers,
        Permissions.ManageRoles,
    ];

    public static IReadOnlyDictionary<Guid, string> Names { get; } = new Dictionary<Guid, string>
    {
        [AdminId] = Admin,
        [ManagerId] = Manager,
        [ModeratorId] = Moderator,
        [UserId] = User,
        [GuestId] = Guest,
    };

    public static IReadOnlyDictionary<Guid, IReadOnlyList<string>> PermissionsByRole { get; } = new Dictionary<Guid, IReadOnlyList<string>>
    {
        [AdminId] = AdminPermissions,
        [ManagerId] = ManagerPermissions,
        [ModeratorId] = ModeratorPermissions,
        [UserId] = UserPermissions,
        [GuestId] = GuestPermissions,
    };
}
