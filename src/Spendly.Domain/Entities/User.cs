using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        protected User() { }

        private User(string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidDomainException("Email is required.");

            Email = email;
            PasswordHash = passwordHash;
        }

        public static User Create(string email, string passwordHash)
        {
            return new User(email, passwordHash);
        }
    }
}
