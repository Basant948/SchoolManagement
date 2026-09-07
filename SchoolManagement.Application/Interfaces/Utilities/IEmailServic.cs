using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces.Utilities
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}