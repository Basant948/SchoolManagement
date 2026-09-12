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
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public AuthController(
            IAuthenticationService authenticationService,
            IUserService userService,
            IPasswordService passwordService,
            ITokenService tokenService)
        {
            _authenticationService = authenticationService;
            _userService = userService;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var (succeeded, isLockedOut, errors) = await _authenticationService.SignInAsync(
                request.Identifier, request.Password, request.RememberMe);

            if (isLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, new { message = "Account is locked. Try again later." });
            }

            if (!succeeded)
            {
                return Unauthorized(new { errors });
            }

            var user = await _userService.GetUserByIdentifierAsync(request.Identifier);

            var (token, expiresAtUtc) = _tokenService.GenerateToken(user!, request.RememberMe);

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

            var user = await _userService.GetUserByIdAsync(userId);
            return user == null ? NotFound() : Ok(user);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { message = "First name and last name are required." });
            }

            var (succeeded, errors) = await _userService.UpdateProfileAsync(
                userId, request.FirstName, request.LastName, request.PhoneNumber);

            if (!succeeded)
            {
                return BadRequest(new { errors });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            return Ok(user);
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Current password and new password are required." });
            }

            var (succeeded, errors) = await _passwordService.ChangePasswordAsync(
                userId, request.CurrentPassword, request.NewPassword);

            if (!succeeded)
            {
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Password changed successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            var (succeeded, message) = await _passwordService.ForgotPasswordAsync(request.Email);
            return Ok(new { message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Email, token and new password are required." });
            }

            var (succeeded, errors) = await _passwordService.ResetPasswordAsync(
                request.Email, request.Token, request.NewPassword);

            if (!succeeded)
            {
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}