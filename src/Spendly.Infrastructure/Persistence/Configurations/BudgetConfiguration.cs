using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Domain.Entities;

namespace Spendly.Infrastructure.Persistence.Configurations
{
    public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.UserId)
                   .IsRequired();

            builder.Property(b => b.Category)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.MonthlyLimit)
                   .HasColumnType("decimal(18,2)")
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(b => b.Year)
                   .IsRequired();

            builder.Property(b => b.Month)
                   .IsRequired();

            builder.Property(b => b.CreatedAt)
                   .IsRequired();

            // FK → Users
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Índice compuesto para búsquedas rápidas por usuario/mes
            builder.HasIndex(b => new { b.UserId, b.Year, b.Month })
                   .HasDatabaseName("IX_Budgets_UserYearMonth");

            // Índice único: un solo presupuesto por categoría/mes/usuario
            builder.HasIndex(b => new { b.UserId, b.Category, b.Year, b.Month })
                   .IsUnique()
                   .HasDatabaseName("IX_Budgets_UserCategoryYearMonth");
        }
    }
}
