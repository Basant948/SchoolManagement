using Microsoft.AspNetCore.Identity;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser, IAuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
        public string? UpdatedBy { get; set; }

    }
}
