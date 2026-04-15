using Spendly.Domain.Entities;

namespace Spendly.Application.Interfaces
{
    // Repository interface for managing Expense entities
    public interface IExpenseRepository
    {
        Task AddAsync(Expense expense);
        Task<bool> DeleteAsync(int id);
        Task UpdateAsync(Expense expense);

        Task<Expense?> GetByIdAsync(int id);

        Task<IEnumerable<Expense>> GetAllAsync(
            int userId,
            string? category,
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            decimal? minAmount,
            decimal? maxAmount,
            int page,
            int pageSize
            );

        Task<int> CountAsync(int userId, string? category, string? search, DateTime? dateFrom, DateTime? dateTo, decimal? minAmount, decimal? maxAmount);


        // ─── Métodos para Dashboard ───

        /// <summary>
        /// Obtiene todos los gastos de un usuario en un rango de fechas.
        /// </summary>
        Task<IEnumerable<Expense>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Suma total de gastos por categoría en un rango de fechas.
        /// </summary>
        Task<Dictionary<string, decimal>> GetTotalByCategoryAsync(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtiene los últimos N gastos de un usuario.
        /// </summary>
        Task<IEnumerable<Expense>> GetRecentAsync(int userId, int count);

        /// <summary>
        /// Suma total de gastos en un rango de fechas.
        /// </summary>
        Task<decimal> GetTotalAmountAsync(int userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtiene gastos agrupados por mes (últimos N meses).
        /// </summary>
        Task<Dictionary<DateTime, decimal>> GetMonthlyTotalsAsync(int userId, int monthsBack);
    }
}
