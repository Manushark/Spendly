using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    /// <summary>
    /// User entity with Google OAuth support
    /// </summary>
    public class User
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string? PasswordHash { get; private set; }

        // Google OAuth properties
        public string? GoogleId { get; private set; }
        public string? DisplayName { get; private set; }
        public string? ProfilePicture { get; private set; }
        public AuthProvider AuthProvider { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        protected User() { }

        // Constructor original
        private User(string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidDomainException("Email is required.");

            Email = email;
            PasswordHash = passwordHash;
            AuthProvider = AuthProvider.Local;
        }

        /// <summary>
        /// Create user with traditional email/password
        /// </summary>
        public static User Create(string email, string passwordHash)
        {
            return new User(email, passwordHash);
        }

        /// <summary>
        /// Create user with Google authentication
        /// </summary>
        public static User CreateWithGoogle(
            string email,
            string googleId,
            string displayName = null,
            string profilePicture = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidDomainException("Email is required.");

            if (string.IsNullOrWhiteSpace(googleId))
                throw new InvalidDomainException("Google ID is required.");

            return new User
            {
                Email = email,
                GoogleId = googleId,
                DisplayName = displayName ?? email.Split('@')[0],
                ProfilePicture = profilePicture,
                AuthProvider = AuthProvider.Google,
                PasswordHash = null, // No password for Google auth
                LastLoginAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Update Google information
        /// </summary>
        public void UpdateGoogleInfo(string googleId, string displayName, string profilePicture)
        {
            if (!string.IsNullOrEmpty(googleId))
                GoogleId = googleId;

            if (!string.IsNullOrEmpty(displayName))
                DisplayName = displayName;

            if (!string.IsNullOrEmpty(profilePicture))
                ProfilePicture = profilePicture;

            LastLoginAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Update last login timestamp
        /// </summary>
        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Verify password for traditional login
        /// </summary>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(PasswordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        /// <summary>
        /// Check if user can login with password
        /// </summary>
        public bool CanLoginWithPassword()
        {
            return AuthProvider == AuthProvider.Local && !string.IsNullOrEmpty(PasswordHash);
        }

        /// <summary>
        /// Check if user can login with Google
        /// </summary>
        public bool CanLoginWithGoogle()
        {
            return AuthProvider == AuthProvider.Google || !string.IsNullOrEmpty(GoogleId);
        }
    }

    /// <summary>
    /// Authentication provider type
    /// </summary>
    public enum AuthProvider
    {
        Local = 1,      // Email/Password
        Google = 2,     // Google OAuth
        Mixed = 3       // Both methods available
    }
}
