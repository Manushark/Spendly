using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Domain.Entities;

namespace Spendly.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId)
                   .IsRequired();

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Icon)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Color)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(c => c.IsDefault)
                   .IsRequired();

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            // FK → Users
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Index for fast user category lookup
            builder.HasIndex(c => c.UserId)
                   .HasDatabaseName("IX_Categories_UserId");

            // Unique: one category name per user
            builder.HasIndex(c => new { c.UserId, c.Name })
                   .IsUnique()
                   .HasDatabaseName("IX_Categories_UserName");
        }
    }
}
