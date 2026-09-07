using Microsoft.Extensions.Configuration;
using SchoolManagement.Application.Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SchoolManagement.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpSection = _configuration.GetSection("Smtp");

            var host = smtpSection["Host"] ?? throw new InvalidOperationException("Smtp:Host is missing.");
            var port = int.Parse(smtpSection["Port"] ?? "587");
            var username = smtpSection["Username"] ?? throw new InvalidOperationException("Smtp:Username is missing.");
            var password = smtpSection["Password"] ?? throw new InvalidOperationException("Smtp:Password is missing.");
            var fromEmail = smtpSection["FromEmail"] ?? username;
            var fromName = smtpSection["FromName"] ?? "School Management";
            var enableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
