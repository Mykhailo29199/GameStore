using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLogicLayer.Models.UserModels;
using GameStore.Shared.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gamestore.WebAPI.Security;

/// <summary>
/// Creates access tokens. Every permission of the user is added as a separate claim,
/// so endpoints are protected by claim-based policies.
/// </summary>
public class JwtTokenFactory(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public string Create(AuthenticatedUserModel user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.Name),
        };

        claims.AddRange(user.Permissions.Select(permission => new Claim(Permissions.ClaimType, permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
