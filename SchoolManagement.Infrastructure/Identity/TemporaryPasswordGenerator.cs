using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SchoolManagement.Infrastructure.Identity
{
    internal static class TemporaryPasswordGenerator
    {
        public static string Generate()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; 
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string all = upper + lower + digits;

            var bytes = new byte[12];
            RandomNumberGenerator.Fill(bytes);

            var chars = new char[12];
            chars[0] = upper[bytes[0] % upper.Length];
            chars[1] = lower[bytes[1] % lower.Length];
            chars[2] = digits[bytes[2] % digits.Length];

            for (var i = 3; i < chars.Length; i++)
            {
                chars[i] = all[bytes[i] % all.Length];
            }

            for (var i = chars.Length - 1; i > 0; i--)
            {
                var j = bytes[i] % (i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}
