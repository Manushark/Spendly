using Spendly.Domain.Exceptions;
using BCrypt.Net;


namespace Spendly.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string? FullName { get; private set; }
        public string PreferredCurrency { get; private set; } = "USD";
        public string TimeZone { get; private set; } = "UTC";
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        protected User() { }

        private User(string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidDomainException("Email is required.");

            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void UpdateProfile(string? fullName, string preferredCurrency, string timeZone)
        {
            if (string.IsNullOrWhiteSpace(preferredCurrency))
                throw new InvalidDomainException("Preferred currency is required.");

            if (preferredCurrency.Length > 10)
                throw new InvalidDomainException("Currency code cannot exceed 10 characters.");

            if (string.IsNullOrWhiteSpace(timeZone))
                throw new InvalidDomainException("Time zone is required.");

            if (fullName != null && fullName.Length > 100)
                throw new InvalidDomainException("Full name cannot exceed 100 characters.");

            FullName = fullName;
            PreferredCurrency = preferredCurrency;
            TimeZone = timeZone;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string currentPassword, string newPasswordHash)
        {
            if (!VerifyPassword(currentPassword))
                throw new InvalidCredentialsException("Current password is incorrect.");

            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public static User Create(string email, string passwordHash)
        {
            return new User(email, passwordHash);
        }
    }
}
