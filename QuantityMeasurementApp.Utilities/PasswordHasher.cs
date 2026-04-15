using System.Security.Cryptography;
using System.Text;

namespace QuantityMeasurementApp.Utilities
{
    public class PasswordHasher
    {
        // Generate Salt
        public static string GenerateSalt()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        // Hash Password with Salt
        public static string HashPassword(string password, string salt)
        {
            var combined = password + salt;
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(bytes);
        }
    }
}
