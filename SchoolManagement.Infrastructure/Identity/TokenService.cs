using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Application.DTOs.Identity;
using SchoolManagement.Application.Interfaces.Services;

namespace SchoolManagement.Infrastructure.Identity
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAtUtc) GenerateToken(UserInfoDto user, bool rememberMe = false)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection["Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured in appsettings.");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var minutes) ? minutes : 60;
            var rememberMeExpiryMinutes = int.TryParse(jwtSection["RememberMeExpiryMinutes"], out var rmMinutes)
                ? rmMinutes
                : 60 * 24 * 7; 

            var effectiveExpiryMinutes = rememberMe ? rememberMeExpiryMinutes : expiryMinutes;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new("firstName", user.FirstName),
                new("lastName", user.LastName)
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            }
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber));
            }

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(effectiveExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expiresAtUtc);
        }
    }
}