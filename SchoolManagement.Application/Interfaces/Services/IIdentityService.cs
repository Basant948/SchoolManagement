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
        Task SignOutAsync();
    }
}