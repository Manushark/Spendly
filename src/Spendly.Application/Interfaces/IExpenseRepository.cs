using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    // Repository interface for managing Expense entities
    public interface IExpenseRepository
    {
        void Add(Expense expense); 
        List<Expense> GetAll();
        Expense? GetById(int id);
        bool Delete(int id);
    }
}
