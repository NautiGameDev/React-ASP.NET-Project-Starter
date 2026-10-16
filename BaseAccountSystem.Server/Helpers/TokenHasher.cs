using System.Security.Cryptography;
using System.Text;

namespace BaseAccountSystem.Server.Helpers
{
    public static class TokenHasher
    {
        public static string HashToken(string rawRefreshToken)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(rawRefreshToken);
            byte[] hashBytes = SHA256.HashData(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
