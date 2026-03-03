using Spendly.Application.DTOs.Budget;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Budgets
{
    public class UpdateBudgetUseCase
    {
        private readonly IBudgetRepository _repo;

        public UpdateBudgetUseCase(IBudgetRepository repo) => _repo = repo;

        public void Execute(int userId, int id, UpdateBudgetDto dto)
        {
            var budget = _repo.GetById(id)
                ?? throw new InvalidDomainException($"Budget {id} not found.");

            budget.EnsureOwnership(userId);
            budget.Update(dto.Category, dto.MonthlyLimit, dto.Year, dto.Month);
            _repo.Update(budget);
        }
    }

    public class DeleteBudgetUseCase
    {
        private readonly IBudgetRepository _repo;

        public DeleteBudgetUseCase(IBudgetRepository repo) => _repo = repo;

        public bool Execute(int userId, int id)
        {
            var budget = _repo.GetById(id);
            if (budget == null) return false;

            budget.EnsureOwnership(userId);
            return _repo.Delete(id);
        }
    }

    public class GetBudgetByIdUseCase
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly IExpenseRepository _expenseRepo;

        public GetBudgetByIdUseCase(IBudgetRepository budgetRepo, IExpenseRepository expenseRepo)
        {
            _budgetRepo = budgetRepo;
            _expenseRepo = expenseRepo;
        }

        public BudgetResponseDto? Execute(int userId, int id)
        {
            var budget = _budgetRepo.GetById(id);
            if (budget == null) return null;

            budget.EnsureOwnership(userId);

            var spent = _expenseRepo.GetAll(userId, budget.Category, page: 1, pageSize: 10000)
                .Where(e => e.Date.Year == budget.Year && e.Date.Month == budget.Month)
                .Sum(e => e.Amount.Value);

            var remaining = budget.MonthlyLimit - spent;
            var percentageUsed = budget.MonthlyLimit > 0 ? (spent / budget.MonthlyLimit) * 100 : 0;

            return new BudgetResponseDto
            {
                Id = budget.Id,
                Category = budget.Category,
                MonthlyLimit = budget.MonthlyLimit,
                Year = budget.Year,
                Month = budget.Month,
                Spent = spent,
                Remaining = remaining,
                PercentageUsed = percentageUsed,
                IsOverBudget = spent > budget.MonthlyLimit,
                CreatedAt = budget.CreatedAt
            };
        }
    }
}
