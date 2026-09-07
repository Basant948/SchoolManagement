using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Common;
using SchoolManagement.Application.DTOs.Identity;
using SchoolManagement.Application.DTOs.Users;
using SchoolManagement.Application.Interfaces.Services;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public UsersController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserRequestDto request)
        {
            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
            var hasPhone = !string.IsNullOrWhiteSpace(request.PhoneNumber);

            if (!hasEmail && !hasPhone)
            {
                return BadRequest(new { message = "Either an email or a phone number is required." });
            }

            if (!AppRoles.Assignable.Contains(request.Role))
            {
                return BadRequest(new { message = $"Role must be one of: {string.Join(", ", AppRoles.Assignable)}." });
            }

            var identifier = hasEmail ? request.Email! : request.PhoneNumber!;
            if (await _identityService.IdentifierExistsAsync(identifier))
            {
                return Conflict(new { message = "An account with this email or phone number already exists." });
            }

            var (succeeded, userId, errors) = await _identityService.CreateUserAsync(new CreateIdentityUserDto
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role
            });

            if (!succeeded)
            {
                return BadRequest(new { errors });
            }

            var user = await _identityService.GetUserByIdAsync(userId!);
            return CreatedAtAction(nameof(GetById), new { id = userId }, user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _identityService.GetUserByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }
    }
}