using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SchoolManagement.Application.DTOs.Identity;
using SchoolManagement.Application.Interfaces.Services;

namespace SchoolManagement.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<bool> IdentifierExistsAsync(string identifier)
        {
            var user = await _userManager.FindByNameAsync(identifier);
            return user != null;
        }

        public async Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateIdentityUserDto dto)
        {
            var hasEmail = !string.IsNullOrWhiteSpace(dto.Email);
            var hasPhone = !string.IsNullOrWhiteSpace(dto.PhoneNumber);

            if (!hasEmail && !hasPhone)
            {
                return (false, null, new[] { "Either an email or a phone number is required." });
            }

            var identifier = hasEmail ? dto.Email! : dto.PhoneNumber!;

            var user = new ApplicationUser
            {
                UserName = identifier,
                Email = hasEmail ? dto.Email : null,
                PhoneNumber = dto.PhoneNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                return (false, null, createResult.Errors.Select(e => e.Description));
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _userManager.DeleteAsync(user);
                    return (false, null, new[] { $"Role '{dto.Role}' does not exist." });
                }

                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return (true, user.Id, Array.Empty<string>());
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(string userId, string firstName, string lastName, string? phoneNumber)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found." });
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);
            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> AddToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(role)) return false;
            if (await _userManager.IsInRoleAsync(user, role)) return true;

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? null : await MapToUserInfoAsync(user);
        }

        public async Task<UserInfoDto?> GetUserByIdentifierAsync(string identifier)
        {
            var user = await _userManager.FindByNameAsync(identifier);
            return user == null ? null : await MapToUserInfoAsync(user);
        }

        public async Task<List<UserInfoDto>> GetUsersByIdsAsync(IEnumerable<string> userIds)
        {
            var results = new List<UserInfoDto>();
            foreach (var id in userIds)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    results.Add(await MapToUserInfoAsync(user));
                }
            }
            return results;
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

        public Task SignOutAsync()
        {
            return Task.CompletedTask;
        }

        private async Task<UserInfoDto> MapToUserInfoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            };
        }
    }
}