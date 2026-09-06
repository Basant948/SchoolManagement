using SchoolManagement.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<bool> EmailExistsAsync(string email);

        Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateIdentityUserDto dto);
        Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(string userId, string firstName, string lastName, string? phoneNumber);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> AddToRoleAsync(string userId, string role);

        Task<UserInfoDto?> GetUserByIdAsync(string userId);
        Task<List<UserInfoDto>> GetUsersByIdsAsync(IEnumerable<string> userIds);

        Task<(bool Succeeded, bool IsLockedOut, IEnumerable<string> Errors)> SignInAsync(string email, string password, bool rememberMe);
        Task SignOutAsync();
    }
}
