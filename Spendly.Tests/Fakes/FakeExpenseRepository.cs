//using Spendly.Application.Interfaces;
//using Spendly.Domain.Entities;

//namespace Spendly.Tests.Fakes
//{
//    public class FakeExpenseRepository : IExpenseRepository
//    {
//        private readonly List<Expense> _expenses = new();

//        public void Add(Expense expense)
//        {
//            _expenses.Add(expense);
//        }

//        public Expense? GetById(int id)
//        {
//            return _expenses.FirstOrDefault(e => e.Id == id);
//        }

//        public IEnumerable<Expense> GetAll(string? category, int page, int pageSize)
//        {
//            return _expenses;
//        }


//        public void Update(Expense expense)
//        {
//            // No hace nada porque el objeto ya está en memoria
//        }

//        public void Delete(Expense expense)
//        {
//            _expenses.Remove(expense);
//        }

//        public bool Delete(int id) // Added missing method to implement the interface
//        {
//            var expense = GetById(id);
//            if (expense != null)
//            {
//                _expenses.Remove(expense);
//                return true;
//            }
//            return false;
//        }
//    }
//}
