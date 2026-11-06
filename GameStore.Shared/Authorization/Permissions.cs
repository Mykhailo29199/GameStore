namespace GameStore.Shared.Authorization;

/// <summary>
/// Permissions that can be assigned to roles. Each permission is issued as a claim in the access token.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    public const string AddGame = "AddGame";
    public const string UpdateGame = "UpdateGame";
    public const string DeleteGame = "DeleteGame";
    public const string ViewDeletedGames = "ViewDeletedGames";
    public const string UpdateDeletedGame = "UpdateDeletedGame";

    public const string AddGenre = "AddGenre";
    public const string UpdateGenre = "UpdateGenre";
    public const string DeleteGenre = "DeleteGenre";

    public const string AddPlatform = "AddPlatform";
    public const string UpdatePlatform = "UpdatePlatform";
    public const string DeletePlatform = "DeletePlatform";

    public const string AddPublisher = "AddPublisher";
    public const string UpdatePublisher = "UpdatePublisher";
    public const string DeletePublisher = "DeletePublisher";

    public const string BuyGame = "BuyGame";
    public const string ViewOrders = "ViewOrders";
    public const string UpdateOrder = "UpdateOrder";
    public const string ShipOrder = "ShipOrder";
    public const string ViewOrderHistory = "ViewOrderHistory";

    public const string AddComment = "AddComment";
    public const string DeleteComment = "DeleteComment";
    public const string BanUser = "BanUser";
    public const string ManageDeletedGameComments = "ManageDeletedGameComments";

    public const string ManageUsers = "ManageUsers";
    public const string ManageRoles = "ManageRoles";

    public static IReadOnlyList<string> All { get; } =
    [
        AddGame, UpdateGame, DeleteGame, ViewDeletedGames, UpdateDeletedGame,
        AddGenre, UpdateGenre, DeleteGenre,
        AddPlatform, UpdatePlatform, DeletePlatform,
        AddPublisher, UpdatePublisher, DeletePublisher,
        BuyGame, ViewOrders, UpdateOrder, ShipOrder, ViewOrderHistory,
        AddComment, DeleteComment, BanUser, ManageDeletedGameComments,
        ManageUsers, ManageRoles,
    ];
}
