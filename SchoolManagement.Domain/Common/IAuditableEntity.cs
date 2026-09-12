using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Domain.Common
{
    public interface IAuditableEntity
    {
        DateTime CreatedAtUtc { get; set; }
        string? CreatedBy { get; set; }

        DateTime? UpdatedAtUtc { get; set; }
        string? UpdatedBy { get; set; }
    }
}
