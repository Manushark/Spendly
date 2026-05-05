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

        /// <summary>
        /// Cuenta la cantidad de gastos de un usuario que pertenecen a una categoría específica.
        /// </summary>
        Task<int> CountByCategoryAsync(int userId, string categoryName);

        /// <summary>
        /// Actualiza el nombre de categoría en todos los gastos de un usuario.
        /// </summary>
        Task UpdateCategoryNameAsync(int userId, string oldName, string newName);

        /// <summary>
        /// Verifica si ya existe un gasto generado para un usuario con la misma descripción, categoría y fecha.
        /// Se usa para prevenir duplicación en la generación de gastos recurrentes.
        /// </summary>
        Task<bool> ExistsByRecurrenceOnDateAsync(int userId, string description, string category, DateTime date);

        /// <summary>
        /// Obtiene la moneda predominante de los gastos de un usuario en una categoría y rango de fechas.
        /// </summary>
        Task<string> GetPredominantCurrencyAsync(int userId, string category, DateTime startDate, DateTime endDate);
    }
}
