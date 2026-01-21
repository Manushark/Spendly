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

            builder.Property(e => e.Description)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Category)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Date)
                   .IsRequired();

            ///MONEY MAPPING 
            builder.Property(e => e.Amount)
                   .HasConversion(
                        money => money.Value,              // Money → decimal (guardar)
                        value => Money.FromDecimal(value)  // decimal → Money (leer)
                   )
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();
        }
    }
}
