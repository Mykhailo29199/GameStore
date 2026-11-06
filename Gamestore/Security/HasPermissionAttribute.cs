using Microsoft.AspNetCore.Authorization;

namespace Gamestore.WebAPI.Security;

/// <summary>
/// Requires the permission claim with the given value. A policy with the permission name is registered for every permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission)
{
}
