using SchoolManagement.Application.DTOs.Identity;
using SchoolManagement.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<bool> IdentifierExistsAsync(string identifier);

        Task<(bool Succeeded, string? UserId, string? TemporaryPassword, IEnumerable<string> Errors)> CreateUserAsync(CreateIdentityUserDto dto);

        Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(string userId, string firstName, string lastName, string? phoneNumber);
        Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateProfileAsync(string userId, string firstName, string lastName, string? phoneNumber);
        Task<UserInfoDto?> GetUserByIdAsync(string userId, bool includeInactive = false);
        Task<UserInfoDto?> GetUserByIdentifierAsync(string identifier);
        Task<List<UserInfoDto>> GetUsersByIdsAsync(IEnumerable<string> userIds);
        Task<(List<UserInfoDto> Items, int TotalCount)> GetUsersAsync(UserFilterDto filter);

        Task<(bool Succeeded, IEnumerable<string> Errors)> DeactivateUserAsync(string userId);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ActivateUserAsync(string userId);
    }
}
