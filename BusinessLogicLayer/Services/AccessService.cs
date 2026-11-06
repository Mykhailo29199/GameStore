using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Authorization;

namespace BusinessLogicLayer.Services;

/// <summary>
/// Decides whether the current user may open a front-end page or use a page action.
/// Read-only pages are available to everyone, including guests; unknown pages are denied.
/// </summary>
public class AccessService(ICurrentUser currentUser, IProductCatalog productCatalog) : IAccessService
{
    private static readonly HashSet<string> PublicPages = new(StringComparer.OrdinalIgnoreCase)
    {
        "Games", "Genres", "Genre", "Platforms", "Platform", "Publishers", "Publisher", "Login",
    };

    private static readonly Dictionary<string, string> PagePermissions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AddGame"] = Permissions.AddGame,
        ["UpdateGame"] = Permissions.UpdateGame,
        ["DeleteGame"] = Permissions.DeleteGame,

        ["AddGenre"] = Permissions.AddGenre,
        ["UpdateGenre"] = Permissions.UpdateGenre,
        ["DeleteGenre"] = Permissions.DeleteGenre,

        ["AddPlatform"] = Permissions.AddPlatform,
        ["UpdatePlatform"] = Permissions.UpdatePlatform,
        ["DeletePlatform"] = Permissions.DeletePlatform,

        ["AddPublisher"] = Permissions.AddPublisher,
        ["UpdatePublisher"] = Permissions.UpdatePublisher,
        ["DeletePublisher"] = Permissions.DeletePublisher,

        ["Buy"] = Permissions.BuyGame,
        ["Basket"] = Permissions.BuyGame,
        ["MakeOrder"] = Permissions.BuyGame,
        ["Orders"] = Permissions.ViewOrders,
        ["Order"] = Permissions.ViewOrders,
        ["UpdateOrder"] = Permissions.UpdateOrder,
        ["ShipOrder"] = Permissions.ShipOrder,
        ["History"] = Permissions.ViewOrderHistory,

        ["AddComment"] = Permissions.AddComment,
        ["ReplyComment"] = Permissions.AddComment,
        ["QuoteComment"] = Permissions.AddComment,
        ["DeleteComment"] = Permissions.DeleteComment,
        ["BanComment"] = Permissions.BanUser,

        ["Users"] = Permissions.ManageUsers,
        ["User"] = Permissions.ManageUsers,
        ["AddUser"] = Permissions.ManageUsers,
        ["UpdateUser"] = Permissions.ManageUsers,
        ["DeleteUser"] = Permissions.ManageUsers,

        ["Roles"] = Permissions.ManageRoles,
        ["Role"] = Permissions.ManageRoles,
        ["AddRole"] = Permissions.ManageRoles,
        ["UpdateRole"] = Permissions.ManageRoles,
        ["DeleteRole"] = Permissions.ManageRoles,
    };

    // Pages whose target id is a game key: a deleted game needs an additional permission.
    private static readonly Dictionary<string, string> DeletedGamePermissions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Game"] = Permissions.ViewDeletedGames,
        ["Comments"] = Permissions.ViewDeletedGames,
        ["UpdateGame"] = Permissions.UpdateDeletedGame,
        ["AddComment"] = Permissions.ManageDeletedGameComments,
    };

    // A deleted game cannot be bought or deleted again, whatever permissions the user has.
    private static readonly HashSet<string> PagesUnavailableForDeletedGame = new(StringComparer.OrdinalIgnoreCase)
    {
        "Buy", "DeleteGame",
    };

    public async Task<bool> CheckAccessAsync(string targetPage, string? targetId)
    {
        if (string.IsNullOrWhiteSpace(targetPage))
        {
            return false;
        }

        if (PagePermissions.TryGetValue(targetPage, out var permission) && !currentUser.HasPermission(permission))
        {
            return false;
        }

        var isGamePage = DeletedGamePermissions.ContainsKey(targetPage) || PagesUnavailableForDeletedGame.Contains(targetPage);

        return isGamePage && !string.IsNullOrWhiteSpace(targetId)
            ? await CheckGameAccessAsync(targetPage, targetId)
            : IsKnownPage(targetPage);
    }

    private static bool IsKnownPage(string targetPage)
    {
        return PagePermissions.ContainsKey(targetPage)
            || PublicPages.Contains(targetPage)
            || DeletedGamePermissions.ContainsKey(targetPage);
    }

    /// <summary>
    /// A game that is not deleted needs no additional permission. A deleted game cannot be bought or deleted again,
    /// and the other game pages require the permission for deleted games.
    /// </summary>
    private async Task<bool> CheckGameAccessAsync(string targetPage, string gameKey)
    {
        var game = await productCatalog.GetByKeyAsync(gameKey, includeDeleted: true);

        return game is null
            || !game.IsDeleted
            || (!PagesUnavailableForDeletedGame.Contains(targetPage)
                && currentUser.HasPermission(DeletedGamePermissions[targetPage]));
    }
}
