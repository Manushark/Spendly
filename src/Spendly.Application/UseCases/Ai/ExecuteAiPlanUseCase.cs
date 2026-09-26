using Spendly.Application.DTOs.Ai;
using Spendly.Application.DTOs.Budget;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCases.Budgets;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Ai
{
    public class ExecuteAiPlanUseCase
    {
        private readonly CreateExpenseUseCase _createExpenseUseCase;
        private readonly CreateBudgetUseCase _createBudgetUseCase;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ExecuteAiPlanUseCase(
            CreateExpenseUseCase createExpenseUseCase,
            CreateBudgetUseCase createBudgetUseCase,
            IBudgetRepository budgetRepository,
            ITagRepository tagRepository,
            IUserRepository userRepository,
            IDateTimeProvider dateTimeProvider)
        {
            _createExpenseUseCase = createExpenseUseCase;
            _createBudgetUseCase = createBudgetUseCase;
            _budgetRepository = budgetRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<AiExecutionResultDto> ExecuteAsync(int userId, AiFinancialPlanDto plan)
        {
            if (plan == null || !plan.HasActions)
            {
                return new AiExecutionResultDto
                {
                    Success = false,
                    Message = "No expenses or budgets to register."
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var currency = user.PreferredCurrency ?? "USD";
            var localToday = _dateTimeProvider.Today(user.TimeZone);

            var createdExpenses = 0;
            var createdBudgets = 0;
            var totalAmount = 0m;

            // 1. Process Expenses
            foreach (var exp in plan.Expenses)
            {
                if (exp.Amount <= 0) continue;

                var expenseDate = exp.Date == default ? localToday : exp.Date;
                if (expenseDate.Date > _dateTimeProvider.UtcNow.AddHours(14).Date)
                {
                    expenseDate = localToday;
                }

                // Resolve tag IDs if any
                var tagIds = new List<int>();
                if (exp.Tags != null && exp.Tags.Count > 0)
                {
                    foreach (var rawTagName in exp.Tags)
                    {
                        var trimmed = rawTagName.Trim().TrimStart('#');
                        if (string.IsNullOrWhiteSpace(trimmed)) continue;

                        var existingTag = await _tagRepository.GetByNameAsync(userId, trimmed);
                        if (existingTag != null)
                        {
                            tagIds.Add(existingTag.Id);
                        }
                        else
                        {
                            var newTag = Tag.Create(userId, trimmed, "#6366f1");
                            await _tagRepository.AddAsync(newTag);
                            tagIds.Add(newTag.Id);
                        }
                    }
                }

                var createExpenseDto = new CreateExpenseDto
                {
                    Amount = exp.Amount,
                    Currency = currency,
                    Description = string.IsNullOrWhiteSpace(exp.Description) ? "Expense via AI Copilot" : exp.Description,
                    Category = string.IsNullOrWhiteSpace(exp.Category) ? "Other" : exp.Category,
                    Date = expenseDate,
                    TagIds = tagIds
                };

                await _createExpenseUseCase.ExecuteAsync(userId, createExpenseDto);
                createdExpenses++;
                totalAmount += exp.Amount;
            }

            // 2. Process Budgets
            foreach (var b in plan.Budgets)
            {
                if (b.MonthlyLimit <= 0) continue;

                var year = b.Year > 0 ? b.Year : localToday.Year;
                var month = b.Month >= 1 && b.Month <= 12 ? b.Month : localToday.Month;
                var category = string.IsNullOrWhiteSpace(b.Category) ? "Other" : b.Category;

                var existingBudget = await _budgetRepository.GetByUserCategoryAndMonthAsync(userId, category, year, month);
                if (existingBudget != null)
                {
                    existingBudget.Update(category, b.MonthlyLimit, year, month);
                    await _budgetRepository.UpdateAsync(existingBudget);
                    createdBudgets++;
                }
                else
                {
                    var createBudgetDto = new CreateBudgetDto
                    {
                        Category = category,
                        MonthlyLimit = b.MonthlyLimit,
                        Year = year,
                        Month = month
                    };
                    await _createBudgetUseCase.ExecuteAsync(userId, createBudgetDto);
                    createdBudgets++;
                }
            }

            return new AiExecutionResultDto
            {
                CreatedExpensesCount = createdExpenses,
                CreatedBudgetsCount = createdBudgets,
                TotalAmount = totalAmount,
                Success = true,
                Message = $"Successfully registered {createdExpenses} expense(s) and {createdBudgets} budget(s)."
            };
        }
    }
}
