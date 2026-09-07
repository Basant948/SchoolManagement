using System.Collections.Generic;

namespace SchoolManagement.Application.DTOs.Identity
{
    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}