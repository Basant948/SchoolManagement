using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SchoolManagement.Application.Interfaces.Services;
using SchoolManagement.Application.Interfaces.Utilities;

namespace SchoolManagement.Infrastructure.Identity
{
    public class PasswordService : IPasswordService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;

        public PasswordService(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IAuditService auditService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _auditService = auditService;
            _configuration = configuration;
        }


        // Change Password

        public async Task<(
            bool Succeeded,
            IEnumerable<string> Errors
        )> ChangePasswordAsync(
            string userId,
            string currentPassword,
            string newPassword)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return (
                    false,
                    new[] { "User not found." }
                );
            }

            var result =
                await _userManager.ChangePasswordAsync(
                    user,
                    currentPassword,
                    newPassword);

            if (result.Succeeded)
            {
                await _auditService.LogAsync(
                    "User",
                    userId,
                    "PasswordChanged");
            }

            return (
                result.Succeeded,
                result.Errors.Select(e => e.Description)
            );
        }

        // Forgot Password

        public async Task<(bool Succeeded, string? Message)> ForgotPasswordAsync(
            string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Don't reveal whether the email exists
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return (
                    true,
                    "If an account with that email exists, " +
                    "a password reset link has been sent."
                );
            }

            // Generate password reset token
            var token =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            // Fake link for testing email delivery
            var resetLink =
                $"https://example.com/reset-password" +
                $"?email={Uri.EscapeDataString(user.Email)}" +
                $"&token={Uri.EscapeDataString(token)}";

            var subject = "Reset Your Password - School Management";

            var body = $"""
        <h2>Password Reset Request</h2>

        <p>Hello {user.FirstName},</p>

        <p>
            We received a request to reset your password.
        </p>

        <p>
            Click the link below to reset your password:
        </p>

        <p>
            <a href="{resetLink}">
                Reset Password
            </a>
        </p>

        <p>
            If you didn't request this, you can safely ignore this email.
        </p>

        <p>
            This link will expire after a short time.
        </p>
        """;

            // Send through Resend
            await _emailService.SendEmailAsync(
                user.Email,
                subject,
                body);

            return (
                true,
                "If an account with that email exists, " +
                "a password reset link has been sent."
            );
        }


        // Reset Password

        public async Task<(
            bool Succeeded,
            IEnumerable<string> Errors
        )> ResetPasswordAsync(
            string email,
            string token,
            string newPassword)
        {
            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return (
                    false,
                    new[] { "Invalid request." }
                );
            }


            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    newPassword);


            if (result.Succeeded)
            {
                await _auditService.LogAsync(
                    "User",
                    user.Id,
                    "PasswordReset");
            }


            return (
                result.Succeeded,
                result.Errors.Select(e => e.Description)
            );
        }


        // Admin Reset Password

        public async Task<(bool Succeeded,
            string? TemporaryPassword,
            IEnumerable<string> Errors
        )> AdminResetPasswordAsync(string userId)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return (
                    false,
                    null,
                    new[] { "User not found." }
                );
            }


            var temporaryPassword =
                TemporaryPasswordGenerator.Generate();


            var token =
                await _userManager
                    .GeneratePasswordResetTokenAsync(user);


            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    temporaryPassword);


            if (!result.Succeeded)
            {
                return (
                    false,
                    null,
                    result.Errors.Select(e => e.Description)
                );
            }


            await _auditService.LogAsync(
                "User",
                userId,
                "PasswordResetByAdmin");


            return (
                true,
                temporaryPassword,
                Array.Empty<string>()
            );
        }
    }
}