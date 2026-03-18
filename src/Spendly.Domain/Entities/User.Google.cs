using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    /// <summary>
    /// Extensión de User para soportar Google OAuth
    /// </summary>
    public partial class User
    {
        // Propiedades para Google OAuth
        public string? GoogleId { get; private set; }
        public string? DisplayName { get; private set; }
        public string? ProfilePicture { get; private set; }
        public AuthProvider AuthProvider { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        /// <summary>
        /// Crear usuario con autenticación de Google
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
        /// Actualizar información de Google
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
        /// Actualizar último login
        /// </summary>
        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Verificar si puede hacer login con password
        /// </summary>
        public bool CanLoginWithPassword()
        {
            return AuthProvider == AuthProvider.Local && !string.IsNullOrEmpty(PasswordHash);
        }

        /// <summary>
        /// Verificar si puede hacer login con Google
        /// </summary>
        public bool CanLoginWithGoogle()
        {
            return AuthProvider == AuthProvider.Google || !string.IsNullOrEmpty(GoogleId);
        }
    }

    /// <summary>
    /// Proveedor de autenticación
    /// </summary>
    public enum AuthProvider
    {
        Local = 1,      // Email/Password tradicional
        Google = 2,     // Google OAuth
        Mixed = 3       // Ambos métodos disponibles
    }
}
