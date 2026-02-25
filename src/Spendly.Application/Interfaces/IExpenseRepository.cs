using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    // Repository interface for managing Expense entities
    public interface IExpenseRepository
    {
        void Add(Expense expense);
        bool Delete(int id);
        void Update(Expense expense);

        Expense? GetById(int id);

        IEnumerable<Expense> GetAll(
            int userId,
            string? category,
            int page,
            int pageSize
            );

        int Count(int userId, string? category);


        // ─── Nuevos métodos para Dashboard ───

        /// <summary>
        /// Obtiene todos los gastos de un usuario en un rango de fechas.
        /// </summary>
        IEnumerable<Expense> GetByDateRange(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Suma total de gastos por categoría en un rango de fechas.
        /// </summary>
        Dictionary<string, decimal> GetTotalByCategory(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtiene los últimos N gastos de un usuario.
        /// </summary>
        IEnumerable<Expense> GetRecent(int userId, int count);

        /// <summary>
        /// Suma total de gastos en un rango de fechas.
        /// </summary>
        decimal GetTotalAmount(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtiene gastos agrupados por mes (últimos N meses).
        /// </summary>
        Dictionary<DateTime, decimal> GetMonthlyTotals(int userId, int monthsBack);
    }
}
