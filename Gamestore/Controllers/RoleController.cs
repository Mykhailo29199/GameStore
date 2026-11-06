using AutoMapper;
using BusinessLogicLayer.Models.UserModels;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Authorization;
using Gamestore.WebAPI.DTOs.RoleDTO;
using Gamestore.WebAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class RoleController(IRoleService roleService, IMapper mapper) : ControllerBase
{
    // The roles list is also needed on the user pages to assign roles.
    [Authorize(Policy = AuthorizationSetup.ManageUsersOrRolesPolicy)]
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
    {
        var roles = await roleService.GetRolesAsync();

        return Ok(mapper.Map<List<RoleDTO>>(roles));
    }

    [Authorize(Policy = AuthorizationSetup.ManageUsersOrRolesPolicy)]
    [HttpGet("roles/{id:guid}")]
    public async Task<ActionResult<RoleDTO>> GetRoleById([FromRoute] Guid id)
    {
        var role = await roleService.GetRoleByIdAsync(id);

        return Ok(mapper.Map<RoleDTO>(role));
    }

    [HasPermission(Permissions.ManageRoles)]
    [HttpGet("roles/permissions")]
    public ActionResult<IEnumerable<string>> GetPermissions()
    {
        return Ok(roleService.GetPermissions());
    }

    [HasPermission(Permissions.ManageRoles)]
    [HttpGet("roles/{id:guid}/permissions")]
    public async Task<ActionResult<IEnumerable<string>>> GetRolePermissions([FromRoute] Guid id)
    {
        return Ok(await roleService.GetRolePermissionsAsync(id));
    }

    [HasPermission(Permissions.ManageRoles)]
    [HttpPost("roles")]
    public async Task<IActionResult> AddRole([FromBody] SaveRoleRequestDTO request)
    {
        await roleService.AddRoleAsync(mapper.Map<RoleModel>(request.Role), request.Permissions);

        return NoContent();
    }

    [HasPermission(Permissions.ManageRoles)]
    [HttpPut("roles")]
    public async Task<IActionResult> UpdateRole([FromBody] SaveRoleRequestDTO request)
    {
        await roleService.UpdateRoleAsync(mapper.Map<RoleModel>(request.Role), request.Permissions);

        return NoContent();
    }

    [HasPermission(Permissions.ManageRoles)]
    [HttpDelete("roles/{id:guid}")]
    public async Task<IActionResult> DeleteRole([FromRoute] Guid id)
    {
        await roleService.DeleteRoleAsync(id);

        return NoContent();
    }
}
