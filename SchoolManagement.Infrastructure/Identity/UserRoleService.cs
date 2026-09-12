using Microsoft.AspNetCore.Identity;
using SchoolManagement.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Infrastructure.Identity
{
    public class UserRoleService : IUserRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;

        public UserRoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
        }

        public async Task<bool> AddToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(role)) return false;
            if (await _userManager.IsInRoleAsync(user, role)) return true;

            var result = await _userManager.AddToRoleAsync(user, role);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("User", userId, "RoleAdded", $"Role: {role}");
            }

            return result.Succeeded;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ChangeUserRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found." });

            if (!await _roleManager.RoleExistsAsync(newRole))
                return (false, new[] { $"Role '{newRole}' does not exist." });

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (result.Succeeded)
            {
                await _auditService.LogAsync(
                    "User", userId, "RoleChanged",
                    $"From: [{string.Join(",", currentRoles)}] To: {newRole}");
            }

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }
    }
}
