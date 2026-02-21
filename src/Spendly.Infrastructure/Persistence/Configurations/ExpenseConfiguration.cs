using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Domain.Entities;
using Spendly.Domain.ValueObjects;

namespace Spendly.Infrastructure.Persistence.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId)
                   .IsRequired();

            builder.Property(e => e.Description)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Category)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Date)
                   .IsRequired();

            builder.Property(e => e.Amount)
                   .HasConversion(
                        money => money.Value,
                        value => Money.FromDecimal(value))
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            // Índice para filtrar gastos por usuario eficientemente
            builder.HasIndex(e => e.UserId);

            // Relación con User (opcional pero recomendado)
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}