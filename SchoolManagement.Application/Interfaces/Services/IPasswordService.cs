using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Application.Interfaces.Services
{
    public interface IPasswordService
    {
        Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<(bool Succeeded, string? Message)> ForgotPasswordAsync(string email);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword);

        Task<(bool Succeeded, string? TemporaryPassword, IEnumerable<string> Errors)> AdminResetPasswordAsync(string userId);
    }
}
