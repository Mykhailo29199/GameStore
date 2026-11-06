namespace BusinessLogicLayer.Services.Interaces;

public interface IAccessService
{
    /// <summary>
    /// Checks whether the current user may open the page (or use the page action) for the given target.
    /// </summary>
    Task<bool> CheckAccessAsync(string targetPage, string? targetId);
}
