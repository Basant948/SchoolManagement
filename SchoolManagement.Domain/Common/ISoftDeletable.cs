using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Domain.Common
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAtUtc { get; set; }
        string? DeletedBy { get; set; }
    }
}
