using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolManagement.Application.DTOs.Identity;

namespace SchoolManagement.Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<bool> IdentifierExistsAsync(string identifier);

        Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateIdentityUserDto dto);
        Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(string userId, string firstName, string lastName, string? phoneNumber);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> AddToRoleAsync(string userId, string role);

        Task<UserInfoDto?> GetUserByIdAsync(string userId);
        Task<UserInfoDto?> GetUserByIdentifierAsync(string identifier);
        Task<List<UserInfoDto>> GetUsersByIdsAsync(IEnumerable<string> userIds);

        Task<(bool Succeeded, bool IsLockedOut, IEnumerable<string> Errors)> SignInAsync(string identifier, string password, bool rememberMe);
        Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateProfileAsync(string userId, string firstName, string lastName, string? phoneNumber);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<(bool Succeeded, string? Message)> ForgotPasswordAsync(string email);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword);
    }
}