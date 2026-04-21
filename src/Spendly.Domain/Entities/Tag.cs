using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    /// <summary>
    /// Tag entity for multi-dimensional expense classification.
    /// </summary>
    public class Tag
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Color { get; private set; } = "#6366f1";

        // Navigation property for many-to-many
        public ICollection<ExpenseTag> ExpenseTags { get; private set; } = [];

        protected Tag() { }

        private Tag(int userId, string name, string color)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDomainException("Tag name cannot be empty.");
            if (name.Length > 30)
                throw new InvalidDomainException("Tag name cannot exceed 30 characters.");

            UserId = userId;
            Name = name.Trim().ToLowerInvariant();
            Color = string.IsNullOrWhiteSpace(color) ? "#6366f1" : color;
        }

        public void Update(string name, string color)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDomainException("Tag name cannot be empty.");
            if (name.Length > 30)
                throw new InvalidDomainException("Tag name cannot exceed 30 characters.");

            Name = name.Trim().ToLowerInvariant();
            Color = string.IsNullOrWhiteSpace(color) ? Color : color;
        }

        public void EnsureOwnership(int userId)
        {
            if (UserId != userId)
                throw new UnauthorizedAccessException($"You do not have access to tag {Id}.");
        }

        public static Tag Create(int userId, string name, string color)
            => new(userId, name, color);
    }

    /// <summary>
    /// Join entity for the Expense ↔ Tag many-to-many relationship.
    /// </summary>
    public class ExpenseTag
    {
        public int ExpenseId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public Expense Expense { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
