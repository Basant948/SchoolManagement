using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Common;
using SchoolManagement.Application.DTOs.Identity;
using SchoolManagement.Application.DTOs.Users;
using SchoolManagement.Application.Interfaces.Services;

namespace SchoolManagement.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRoleService _userRoleService;
    private readonly IPasswordService _passwordService;

    public UsersController(
        IUserService userService,
        IUserRoleService userRoleService,
        IPasswordService passwordService)
    {
        _userService = userService;
        _userRoleService = userRoleService;
        _passwordService = passwordService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserFilterDto filter)
    {
        var (items, totalCount) = await _userService.GetUsersAsync(filter);

        return Ok(new
        {
            totalCount,
            page = filter.Page,
            pageSize = filter.PageSize,
            items
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id, includeInactive: true);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequestDto request)
    {
        var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
        var hasPhone = !string.IsNullOrWhiteSpace(request.PhoneNumber);

        if (!hasEmail && !hasPhone)
            return BadRequest(new { message = "Either an email or a phone number is required." });

        if (!AppRoles.All.Contains(request.Role))
            return BadRequest(new { message = $"Role must be one of: {string.Join(", ", AppRoles.All)}." });

        var identifier = hasEmail ? request.Email! : request.PhoneNumber!;
        if (await _userService.IdentifierExistsAsync(identifier))
            return Conflict(new { message = "An account with this email or phone number already exists." });

        var (succeeded, userId, temporaryPassword, errors) = await _userService.CreateUserAsync(new CreateIdentityUserDto
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role
        });

        if (!succeeded)
            return BadRequest(new { errors });

        var user = await _userService.GetUserByIdAsync(userId!, includeInactive: true);

        return CreatedAtAction(nameof(GetById), new { id = userId }, new
        {
            user,
            temporaryPassword,
            message = temporaryPassword != null
                ? "Account created. Share this temporary password with the user through a secure channel - they should change it after logging in."
                : "Account created. A link to set their password has been emailed to the user."
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserRequestDto request)
    {
        var (succeeded, errors) = await _userService.UpdateUserAsync(
            id, request.FirstName, request.LastName, request.PhoneNumber);

        if (!succeeded)
            return BadRequest(new { errors });

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!AppRoles.All.Contains(request.Role))
                return BadRequest(new { message = $"Role must be one of: {string.Join(", ", AppRoles.All)}." });

            var roleResult = await _userRoleService.ChangeUserRoleAsync(id, request.Role);
            if (!roleResult.Succeeded)
                return BadRequest(new { errors = roleResult.Errors });
        }

        var user = await _userService.GetUserByIdAsync(id, includeInactive: true);
        return Ok(user);
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var (succeeded, errors) = await _userService.DeactivateUserAsync(id);
        return succeeded ? NoContent() : BadRequest(new { errors });
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(string id)
    {
        var (succeeded, errors) = await _userService.ActivateUserAsync(id);
        return succeeded ? NoContent() : BadRequest(new { errors });
    }

    [HttpPatch("{id}/reset-password")]
    public async Task<IActionResult> AdminResetPassword(string id)
    {
        var (succeeded, temporaryPassword, errors) = await _passwordService.AdminResetPasswordAsync(id);

        if (!succeeded)
            return BadRequest(new { errors });

        return Ok(new
        {
            temporaryPassword,
            message = "Password has been reset. Share this temporary password with the user through a secure channel - they should change it after logging in."
        });
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        return Ok(new
        {
            all = AppRoles.All,
        });
    }
}
