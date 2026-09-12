using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Application.Interfaces.Services
{
    public interface IUserRoleService
    {
        Task<bool> AddToRoleAsync(string userId, string role);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ChangeUserRoleAsync(string userId, string newRole);
    }
}
