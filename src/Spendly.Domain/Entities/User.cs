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

        // ── Email notification preferences ──────────────────────────
        /// <summary>Master toggle: disabling this suppresses ALL email notifications.</summary>
        public bool EmailNotificationsEnabled { get; private set; } = true;
        /// <summary>Receive an email when a budget reaches 80% or 100%.</summary>
        public bool BudgetAlertEmailEnabled { get; private set; } = true;
        /// <summary>Receive a weekly spending digest every Monday.</summary>
        public bool WeeklySummaryEmailEnabled { get; private set; } = true;


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

        /// <summary>
        /// Reemplaza la contraseña directamente, sin verificar la actual.
        /// Solo debe llamarse desde el flujo de Reset Password,
        /// donde el usuario ya se identificó con el token de email.
        /// </summary>
        public void SetPasswordHash(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new InvalidDomainException("Password hash cannot be empty.");

            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateNotificationPreferences(
            bool emailNotificationsEnabled,
            bool budgetAlertEmailEnabled,
            bool weeklySummaryEmailEnabled)
        {
            EmailNotificationsEnabled = emailNotificationsEnabled;
            BudgetAlertEmailEnabled = budgetAlertEmailEnabled;
            WeeklySummaryEmailEnabled = weeklySummaryEmailEnabled;
            UpdatedAt = DateTime.UtcNow;
        }

        public static User Create(string email, string passwordHash)
        {
            return new User(email, passwordHash);
        }
    }
}
