using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.DTOs.Auth;
using SchoolManagement.Application.Interfaces.Services;

namespace SchoolManagement.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;

        public AuthController(IIdentityService identityService, ITokenService tokenService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var (succeeded, isLockedOut, errors) = await _identityService.SignInAsync(
                request.Identifier, request.Password, request.RememberMe);

            if (isLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, new { message = "Account is locked. Try again later." });
            }

            if (!succeeded)
            {
                return Unauthorized(new { errors });
            }

            var user = await _identityService.GetUserByIdentifierAsync(request.Identifier);
            var (token, expiresAtUtc) = _tokenService.GenerateToken(user!);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                User = user!
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _identityService.GetUserByIdAsync(userId);
            return user == null ? NotFound() : Ok(user);
        }
    }
}