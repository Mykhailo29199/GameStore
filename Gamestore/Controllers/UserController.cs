using AutoMapper;
using BusinessLogicLayer.Models.UserModels;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Authorization;
using Gamestore.WebAPI.DTOs.RoleDTO;
using Gamestore.WebAPI.DTOs.UserDTO;
using Gamestore.WebAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class UserController(
    IUserService userService,
    IAccessService accessService,
    JwtTokenFactory tokenFactory,
    IMapper mapper) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("users/login")]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
    {
        var user = await userService.LoginAsync(mapper.Map<LoginModel>(request.Model));

        return Ok(new LoginResponseDTO { Token = tokenFactory.Create(user) });
    }

    /// <summary>
    /// Self-registration. The new user gets the User role and is logged in right away.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("users/register")]
    public async Task<ActionResult<LoginResponseDTO>> Register([FromBody] RegisterRequestDTO request)
    {
        var user = await userService.RegisterAsync(request.Login, request.Password);

        return Ok(new LoginResponseDTO { Token = tokenFactory.Create(user) });
    }

    [AllowAnonymous]
    [HttpPost("users/access")]
    public async Task<ActionResult<bool>> CheckAccess([FromBody] AccessRequestDTO request)
    {
        return Ok(await accessService.CheckAccessAsync(request.TargetPage, request.TargetId));
    }

    [HasPermission(Permissions.ManageUsers)]
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
    {
        var users = await userService.GetUsersAsync();

        return Ok(mapper.Map<List<UserDTO>>(users));
    }

    [HasPermission(Permissions.ManageUsers)]
    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserDTO>> GetUserById([FromRoute] Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);

        return Ok(mapper.Map<UserDTO>(user));
    }

    [HasPermission(Permissions.ManageUsers)]
    [HttpGet("users/{id:guid}/roles")]
    public async Task<ActionResult<IEnumerable<RoleDTO>>> GetUserRoles([FromRoute] Guid id)
    {
        var roles = await userService.GetUserRolesAsync(id);

        return Ok(mapper.Map<List<RoleDTO>>(roles));
    }

    [HasPermission(Permissions.ManageUsers)]
    [HttpPost("users")]
    public async Task<IActionResult> AddUser([FromBody] SaveUserRequestDTO request)
    {
        await userService.AddUserAsync(mapper.Map<UserModel>(request.User), request.Roles, request.Password ?? string.Empty);

        return NoContent();
    }

    [HasPermission(Permissions.ManageUsers)]
    [HttpPut("users")]
    public async Task<IActionResult> UpdateUser([FromBody] SaveUserRequestDTO request)
    {
        await userService.UpdateUserAsync(mapper.Map<UserModel>(request.User), request.Roles, request.Password);

        return NoContent();
    }

    [HasPermission(Permissions.ManageUsers)]
    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
    {
        await userService.DeleteUserAsync(id);

        return NoContent();
    }
}
