using Microsoft.AspNetCore.Identity;
using Shop_server.Models;

namespace Shop_server.Security
{
    public class PasswordHasher
    {

        private readonly PasswordHasher<User> _hasher = new();

        public string Hash(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }

        public bool Verify(User user, string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
