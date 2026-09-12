using Microsoft.Extensions.Configuration;
using Resend;
using SchoolManagement.Application.Interfaces.Utilities;

namespace SchoolManagement.Infrastructure.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;

        public ResendEmailService(
            IResend resend,
            IConfiguration configuration)
        {
            _resend = resend;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody)
        {
            var fromEmail =
                _configuration["Resend:FromEmail"]
                ?? throw new InvalidOperationException(
                    "Resend:FromEmail is missing.");

            var fromName =
                _configuration["Resend:FromName"]
                ?? "NVM Boarding School";

            var message = new EmailMessage
            {
                From = $"{fromName} <{fromEmail}>",
                Subject = subject,
                HtmlBody = htmlBody
            };

            message.To.Add(toEmail);

            await _resend.EmailSendAsync(message);
        }
    }
}