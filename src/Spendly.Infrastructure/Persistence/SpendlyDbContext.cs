using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spendly.Domain.Entities;

namespace Spendly.Infrastructure.Persistence
{
    public class SpendlyDbContext : DbContext
    {
        public DbSet<Expense> Expenses { get; set; }

        public SpendlyDbContext(DbContextOptions<SpendlyDbContext> options) : base(options)
        {

        }
    }
}
