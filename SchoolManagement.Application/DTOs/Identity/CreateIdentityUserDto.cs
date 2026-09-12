namespace SchoolManagement.Application.DTOs.Identity
{
    public class CreateIdentityUserDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}