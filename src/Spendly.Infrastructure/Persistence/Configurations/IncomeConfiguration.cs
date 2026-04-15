using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Domain.Entities;
using Spendly.Domain.ValueObjects;

namespace Spendly.Infrastructure.Persistence.Configurations
{
    public class IncomeConfiguration : IEntityTypeConfiguration<Income>
    {
        public void Configure(EntityTypeBuilder<Income> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.UserId)
                   .IsRequired();

            builder.Property(i => i.Source)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.Description)
                   .HasMaxLength(200);

            builder.Property(i => i.Date)
                   .IsRequired();

            builder.Property(i => i.Amount)
                   .HasConversion(
                        money => money.Value,
                        value => Money.FromDecimal(value))
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(i => i.IsRecurring)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(i => i.CreatedAt)
                   .IsRequired();

            // Index for efficient user filtering
            builder.HasIndex(i => i.UserId);

            // Relationship with User
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(i => i.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
