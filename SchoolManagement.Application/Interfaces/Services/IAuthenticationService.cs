using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<(bool Succeeded, bool IsLockedOut, IEnumerable<string> Errors)> SignInAsync(string identifier, string password, bool rememberMe);
    }
}
