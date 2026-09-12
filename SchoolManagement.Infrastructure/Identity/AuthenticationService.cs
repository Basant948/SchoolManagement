using Microsoft.AspNetCore.Identity;
using SchoolManagement.Application.Interfaces.Services;

namespace SchoolManagement.Infrastructure.Identity
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Succeeded, bool IsLockedOut, IEnumerable<string> Errors)> SignInAsync(string identifier, string password, bool rememberMe)
        {
            var user = await _userManager.FindByNameAsync(identifier);
            if (user == null)
            {
                return (false, false, new[] { "Invalid credentials." });
            }

            if (!user.IsActive)
            {
                return (false, false, new[] { "This account has been deactivated." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return (true, false, Array.Empty<string>());
            }

            if (result.IsLockedOut)
            {
                return (false, true, new[] { "Account is locked out. Try again later." });
            }

            return (false, false, new[] { "Invalid credentials." });
        }
    }
}
