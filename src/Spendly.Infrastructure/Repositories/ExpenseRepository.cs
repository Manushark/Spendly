using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spendly.Domain.Entities;
using Spendly.Application.Interfaces;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly SpendlyDbContext _context;
        public ExpenseRepository(SpendlyDbContext dbContext)
        {
            _context = dbContext;
        }

        public void Add(Expense expense)
        {
            _context.Add(expense);
            _context.SaveChanges();
        }

        public List<Expense> GetAll()
        {
            return _context.Set<Expense>().ToList();
        }
        public Expense? GetById(int id)
        {
            return _context.Set<Expense>().FirstOrDefault(e => e.Id == id);
        }


    }
}
