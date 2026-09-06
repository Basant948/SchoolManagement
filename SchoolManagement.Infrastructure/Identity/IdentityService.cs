using SchoolManagement.Application.DTOs.Identity;
using SchoolManagement.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        public Task<bool> AddToRoleAsync(string userId, string role)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateIdentityUserDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<UserInfoDto?> GetUserByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserInfoDto>> GetUsersByIdsAsync(IEnumerable<string> userIds)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Succeeded, bool IsLockedOut, IEnumerable<string> Errors)> SignInAsync(string email, string password, bool rememberMe)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(string userId, string firstName, string lastName, string? phoneNumber)
        {
            throw new NotImplementedException();
        }
    }
}
