using SchoolManagement.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public UserInfoDto User { get; set; } = null!;
    }
}
