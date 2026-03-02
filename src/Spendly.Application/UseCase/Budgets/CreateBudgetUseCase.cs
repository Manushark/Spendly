using Spendly.Application.DTOs.Budget;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Budgets
{
    public class CreateBudgetUseCase
    {
        private readonly IBudgetRepository _repo;

        public CreateBudgetUseCase(IBudgetRepository repo) => _repo = repo;

        public void Execute(int userId, CreateBudgetDto dto)
        {
            // Verificar si ya existe un presupuesto para esta categoría en este mes
            var existing = _repo.GetByUserCategoryAndMonth(userId, dto.Category, dto.Year, dto.Month);
            if (existing != null)
            {
                throw new InvalidDomainException(
                    $"A budget for '{dto.Category}' already exists for {dto.Month}/{dto.Year}. " +
                    $"Please update the existing budget instead.");
            }

            var budget = Budget.Create(userId, dto.Category, dto.MonthlyLimit, dto.Year, dto.Month);
            _repo.Add(budget);
        }
    }
}