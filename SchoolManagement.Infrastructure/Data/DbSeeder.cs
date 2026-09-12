using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Application.Common;
using SchoolManagement.Infrastructure.Identity;

namespace SchoolManagement.Infrastructure.Data
{
    public static class DbSeeder
    {
        private const string AdminEmail = "admin@schoolmanagement.com";
        private const string AdminPassword = "Admin@123";
        private const string AdminFirstName = "System";
        private const string AdminLastName = "Administrator";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in AppRoles.All)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var existingAdmin = await userManager.FindByEmailAsync(AdminEmail);
            if (existingAdmin != null)
            {
                if (!await userManager.IsInRoleAsync(existingAdmin, AppRoles.Admin))
                {
                    await userManager.AddToRoleAsync(existingAdmin, AppRoles.Admin);
                }
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FirstName = AdminFirstName,
                LastName = AdminLastName,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(admin, AdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }

            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}