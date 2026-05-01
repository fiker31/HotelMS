using System.Security.Cryptography;
using System.Text;

namespace HotelMS.Helpers
{
    public static class AuthHelper
    {
        public static string HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToHexString(saltBytes);
            var combined = Encoding.UTF8.GetBytes(salt + password);
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(combined));
            return $"{salt}:{hash}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;
            var salt = parts[0];
            var expectedHash = parts[1];
            var combined = Encoding.UTF8.GetBytes(salt + password);
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(combined));
            return hash == expectedHash;
        }

        public static bool IsAuthenticated(ISession session)
        {
            return session.GetString("UserID") != null;
        }

        public static bool HasRole(ISession session, params string[] roles)
        {
            var role = session.GetString("UserRole");
            return role != null && roles.Contains(role);
        }
    }
}
