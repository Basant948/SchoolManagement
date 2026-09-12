using Microsoft.AspNetCore.Identity;
using SchoolManagement.Application.DTOs.Identity;
using SchoolManagement.Application.DTOs.Users;
using SchoolManagement.Application.Interfaces.Services;
using SchoolManagement.Application.Interfaces.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Infrastructure.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _auditService = auditService;
        }

        public async Task<bool> IdentifierExistsAsync(string identifier)
        {
            var user = await _userManager.FindByNameAsync(identifier);
            return user != null;
        }

        public async Task<(bool Succeeded, string? UserId, string? TemporaryPassword, IEnumerable<string> Errors)> CreateUserAsync(CreateIdentityUserDto dto)
        {
            var hasEmail = !string.IsNullOrWhiteSpace(dto.Email);
            var hasPhone = !string.IsNullOrWhiteSpace(dto.PhoneNumber);

            if (!hasEmail && !hasPhone)
            {
                return (false, null, null, new[] { "Either an email or a phone number is required." });
            }

            var identifier = hasEmail ? dto.Email! : dto.PhoneNumber!;

            var user = new ApplicationUser
            {
                UserName = identifier,
                Email = hasEmail ? dto.Email : null,
                PhoneNumber = dto.PhoneNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return (false, null, null, createResult.Errors.Select(e => e.Description));
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _userManager.DeleteAsync(user);
                    return (false, null, null, new[] { $"Role '{dto.Role}' does not exist." });
                }

                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            string? temporaryPassword = null;

            if (hasEmail)
            {
                try
                {
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var setPasswordLink =
                        $"https://example.com/set-password?email={Uri.EscapeDataString(dto.Email!)}&token={Uri.EscapeDataString(resetToken)}";

                    var subject = "Welcome to School Management System";
                    var body = $@"
                <h2>Welcome, {dto.FirstName}!</h2>
                <p>Your account has been created successfully.</p>
                <p><strong>Login Details:</strong></p>
                <ul>
                    <li><strong>Email / Username:</strong> {dto.Email}</li>
                    <li><strong>Role:</strong> {dto.Role}</li>
                </ul>
                <p>Please set your password using the link below before logging in:</p>
                <p><a href=""{setPasswordLink}"">Set Your Password</a></p>
                <p>Thank you!</p>
                    ";

                    await _emailService.SendEmailAsync(dto.Email!, subject, body);
                }
                catch
                {
                }
            }
            else
            {
                temporaryPassword = TemporaryPasswordGenerator.Generate();
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var setResult = await _userManager.ResetPasswordAsync(user, resetToken, temporaryPassword);

                if (!setResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return (false, null, null, setResult.Errors.Select(e => e.Description));
                }
            }

            await _auditService.LogAsync("User", user.Id, "Created", $"Role: {dto.Role}");

            return (true, user.Id, temporaryPassword, Array.Empty<string>());
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
            user.UpdatedAtUtc = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("User", userId, "Updated");
            }

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateProfileAsync(
               string userId, string firstName, string lastName, string? phoneNumber)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found." });
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.UpdatedAtUtc = DateTime.UtcNow;
            user.UpdatedBy = userId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("User", userId, "ProfileUpdated");
            }

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task<UserInfoDto?> GetUserByIdAsync(string userId, bool includeInactive = false)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            if (!includeInactive && !user.IsActive) return null;

            return await MapToUserInfoAsync(user);
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

        public async Task<(List<UserInfoDto> Items, int TotalCount)> GetUsersAsync(UserFilterDto filter)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }
            else
            {
                query = query.Where(u => u.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                if (!await _roleManager.RoleExistsAsync(filter.Role))
                {
                    return (new List<UserInfoDto>(), 0);
                }

                var usersInRole = await _userManager.GetUsersInRoleAsync(filter.Role);
                var userIdsInRole = usersInRole.Select(u => u.Id).ToList();

                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAtUtc)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<UserInfoDto>();
            foreach (var user in users)
            {
                result.Add(await MapToUserInfoAsync(user));
            }

            return (result, totalCount);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found." });

            if (user.Id == _currentUserService.UserId)
                return (false, new[] { "You cannot deactivate your own account." });

            if (!user.IsActive)
                return (false, new[] { "User is already deactivated." });

            user.IsActive = false;
            user.UpdatedAtUtc = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("User", userId, "Deactivated");
            }

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, new[] { "User not found." });

            if (user.IsActive)
                return (false, new[] { "User is already active." });

            user.IsActive = true;
            user.UpdatedAtUtc = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("User", userId, "Activated");
            }

            return (result.Succeeded, result.Errors.Select(e => e.Description));
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
                Roles = roles,
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }
    }
}
