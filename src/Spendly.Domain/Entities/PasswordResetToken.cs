namespace Spendly.Domain.Entities
{
    /// <summary>
    /// Token de un solo uso para restablecer contraseña.
    /// Expira en 1 hora y se invalida al ser utilizado.
    /// </summary>
    public class PasswordResetToken
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Token { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected PasswordResetToken() { }

        private PasswordResetToken(int userId, string token)
        {
            UserId = userId;
            Token = token;
            ExpiresAt = DateTime.UtcNow.AddHours(1);
            IsUsed = false;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsValid() => !IsUsed && DateTime.UtcNow < ExpiresAt;

        public void MarkAsUsed()
        {
            IsUsed = true;
        }

        public static PasswordResetToken Create(int userId, string token)
            => new(userId, token);
    }
}
