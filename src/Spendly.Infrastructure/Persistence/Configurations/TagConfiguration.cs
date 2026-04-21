using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Domain.Entities;

namespace Spendly.Infrastructure.Persistence.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.UserId).IsRequired();
            builder.Property(t => t.Name).IsRequired().HasMaxLength(30);
            builder.Property(t => t.Color).HasMaxLength(20).HasDefaultValue("#6366f1");

            builder.HasIndex(t => new { t.UserId, t.Name }).IsUnique();

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ExpenseTagConfiguration : IEntityTypeConfiguration<ExpenseTag>
    {
        public void Configure(EntityTypeBuilder<ExpenseTag> builder)
        {
            builder.HasKey(et => new { et.ExpenseId, et.TagId });

            builder.HasOne(et => et.Expense)
                   .WithMany()
                   .HasForeignKey(et => et.ExpenseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(et => et.Tag)
                   .WithMany(t => t.ExpenseTags)
                   .HasForeignKey(et => et.TagId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
