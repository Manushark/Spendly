using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Infrastructure.Persistence.Configurations
{
    public class RecurringExpenseConfiguration : IEntityTypeConfiguration<RecurringExpense>
    {
        public void Configure(EntityTypeBuilder<RecurringExpense> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.UserId)
                   .IsRequired();

            builder.Property(r => r.Description)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.OwnsOne(r => r.Amount, money =>
            {
                money.Property(m => m.Value)
                     .HasColumnName("Amount")
                     .HasColumnType("decimal(18,2)")
                     .HasPrecision(18, 2)
                     .IsRequired();
            });

            builder.Property(r => r.Category)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(r => r.Frequency)
                   .IsRequired()
                   .HasConversion<int>();  // Guardar como int en BD

            builder.Property(r => r.StartDate)
                   .IsRequired()
                   .HasColumnType("date");

            builder.Property(r => r.EndDate)
                   .HasColumnType("date");

            builder.Property(r => r.LastGeneratedDate)
                   .HasColumnType("date");

            builder.Property(r => r.IsActive)
                   .IsRequired();

            builder.Property(r => r.CreatedAt)
                   .IsRequired();

            // FK → Users
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(r => r.UserId)
                   .HasDatabaseName("IX_RecurringExpenses_UserId");

            builder.HasIndex(r => new { r.UserId, r.IsActive })
                   .HasDatabaseName("IX_RecurringExpenses_UserActive");
        }
    }
}
