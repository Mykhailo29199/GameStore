using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLogicLayer.Security;
using GameStore.Shared.Authorization;

namespace Gamestore.WebAPI.Security;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? Id => Guid.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public string? Name => Principal?.FindFirstValue(JwtRegisteredClaimNames.Name);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool HasPermission(string permission)
    {
        return Principal?.HasClaim(Permissions.ClaimType, permission) == true;
    }
}
