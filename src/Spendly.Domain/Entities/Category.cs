using Spendly.Domain.Exceptions;

namespace Spendly.Domain.Entities
{
    /// <summary>
    /// Categoría personalizada de gastos.
    /// Cada usuario tiene su propio catálogo de categorías con iconos y colores.
    /// </summary>
    public class Category
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Icon { get; private set; } = null!;
        public string Color { get; private set; } = null!;
        public bool IsDefault { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Category() { }

        private Category(int userId, string name, string icon, string color, bool isDefault)
        {
            Validate(name, icon, color);
            UserId = userId;
            Name = name;
            Icon = icon;
            Color = color;
            IsDefault = isDefault;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string icon, string color)
        {
            Validate(name, icon, color);
            Name = name;
            Icon = icon;
            Color = color;
        }

        public void EnsureOwnership(int userId)
        {
            if (UserId != userId)
                throw new UnauthorizedAccessException($"Category {Id} does not belong to user {userId}.");
        }

        private static void Validate(string name, string icon, string color)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDomainException("Category name cannot be empty.");

            if (name.Length > 50)
                throw new InvalidDomainException("Category name cannot exceed 50 characters.");

            if (string.IsNullOrWhiteSpace(icon))
                throw new InvalidDomainException("Icon cannot be empty.");

            if (icon.Length > 50)
                throw new InvalidDomainException("Icon cannot exceed 50 characters.");

            if (string.IsNullOrWhiteSpace(color))
                throw new InvalidDomainException("Color cannot be empty.");

            if (color.Length > 10)
                throw new InvalidDomainException("Color cannot exceed 10 characters.");
        }

        public static Category Create(int userId, string name, string icon, string color, bool isDefault = false)
            => new(userId, name, icon, color, isDefault);
    }
}
