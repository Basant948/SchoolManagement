using SchoolManagement.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Application.Interfaces.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAtUtc) GenerateToken(UserInfoDto user, bool rememberMe = false);
    }
}
