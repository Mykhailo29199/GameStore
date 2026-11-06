using System.Text;
using GameStore.Shared.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gamestore.WebAPI.Security;

public static class AuthorizationSetup
{
    /// <summary>
    /// Policy for endpoints that are needed both for user management and for role management.
    /// </summary>
    public const string ManageUsersOrRolesPolicy = "ManageUsersOrRoles";

    private const string BearerPrefix = "Bearer ";

    public static IServiceCollection AddGameStoreAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        services.Configure<JwtOptions>(jwtSection);
        services.AddSingleton<JwtTokenFactory>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep the original claim names ("sub", "name") instead of mapping them to the long XML ones.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };

                options.Events = new JwtBearerEvents
                {
                    // The front-end sends the token in the Authorization header without the "Bearer" scheme.
                    OnMessageReceived = context =>
                    {
                        var header = context.Request.Headers.Authorization.ToString();

                        if (!string.IsNullOrWhiteSpace(header))
                        {
                            context.Token = header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
                                ? header[BearerPrefix.Length..].Trim()
                                : header.Trim();
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(Permissions.ClaimType, permission));
            }

            options.AddPolicy(ManageUsersOrRolesPolicy, policy => policy.RequireAssertion(context =>
                context.User.HasClaim(Permissions.ClaimType, Permissions.ManageUsers) ||
                context.User.HasClaim(Permissions.ClaimType, Permissions.ManageRoles)));
        });

        return services;
    }
}
