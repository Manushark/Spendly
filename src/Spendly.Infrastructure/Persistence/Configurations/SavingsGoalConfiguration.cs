using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Domain.Entities;

namespace Spendly.Infrastructure.Persistence.Configurations
{
    public class SavingsGoalConfiguration : IEntityTypeConfiguration<SavingsGoal>
    {
        public void Configure(EntityTypeBuilder<SavingsGoal> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId).IsRequired();
            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.Property(s => s.TargetAmount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(s => s.CurrentAmount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(s => s.Deadline);
            builder.Property(s => s.Icon).HasMaxLength(50).HasDefaultValue("bi-bullseye");
            builder.Property(s => s.Color).HasMaxLength(20).HasDefaultValue("#6366f1");
            builder.Property(s => s.IsCompleted).IsRequired().HasDefaultValue(false);
            builder.Property(s => s.CreatedAt).IsRequired();

            builder.HasIndex(s => s.UserId);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
