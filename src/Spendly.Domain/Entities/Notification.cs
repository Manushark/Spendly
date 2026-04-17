using Spendly.Domain.Enums;
using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    /// <summary>
    /// Notificación in-app para alertas de presupuesto y otros eventos del sistema.
    /// </summary>
    public class Notification
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Message { get; private set; } = null!;
        public NotificationType Type { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public int? RelatedEntityId { get; private set; }

        protected Notification() { }

        private Notification(int userId, string message, NotificationType type, int? relatedEntityId)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidDomainException("Notification message cannot be empty.");

            UserId = userId;
            Message = message;
            Type = type;
            IsRead = false;
            CreatedAt = DateTime.UtcNow;
            RelatedEntityId = relatedEntityId;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public static Notification Create(int userId, string message, NotificationType type, int? relatedEntityId = null)
            => new(userId, message, type, relatedEntityId);
    }
}
