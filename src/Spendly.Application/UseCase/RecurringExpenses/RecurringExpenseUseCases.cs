using Spendly.Application.DTOs.RecurringExpense;
using Spendly.Application.Interfaces;
using Spendly.Application.Services;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.RecurringExpenses
{
    /// <summary>
    /// Crear recurrencia.
    /// </summary>
    public class CreateRecurringExpenseUseCase
    {
        private readonly IRecurringExpenseRepository _repo;

        public CreateRecurringExpenseUseCase(IRecurringExpenseRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, CreateRecurringExpenseDto dto)
        {
            var recurring = RecurringExpense.Create(
                userId,
                dto.Description,
                dto.Amount,
                dto.Category,
                dto.Frequency,
                dto.StartDate,
                dto.EndDate
            );

            await _repo.AddAsync(recurring);
        }
    }

    /// <summary>
    /// Actualizar recurrencia.
    /// </summary>
    public class UpdateRecurringExpenseUseCase
    {
        private readonly IRecurringExpenseRepository _repo;

        public UpdateRecurringExpenseUseCase(IRecurringExpenseRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int id, UpdateRecurringExpenseDto dto)
        {
            var recurring = await _repo.GetByIdAsync(id)
                ?? throw new InvalidDomainException($"Recurring expense {id} not found.");

            recurring.EnsureOwnership(userId);
            recurring.Update(
                dto.Description,
                dto.Amount,
                dto.Category,
                dto.Frequency,
                dto.StartDate,
                dto.EndDate
            );

            await _repo.UpdateAsync(recurring);
        }
    }

    /// <summary>
    /// Eliminar recurrencia.
    /// </summary>
    public class DeleteRecurringExpenseUseCase
    {
        private readonly IRecurringExpenseRepository _repo;

        public DeleteRecurringExpenseUseCase(IRecurringExpenseRepository repo) => _repo = repo;

        public async Task<bool> ExecuteAsync(int userId, int id)
        {
            var recurring = await _repo.GetByIdAsync(id);
            if (recurring == null) return false;

            recurring.EnsureOwnership(userId);
            return await _repo.DeleteAsync(id);
        }
    }

    /// <summary>
    /// Activar/Pausar recurrencia.
    /// </summary>
    public class ToggleRecurringExpenseUseCase
    {
        private readonly IRecurringExpenseRepository _repo;

        public ToggleRecurringExpenseUseCase(IRecurringExpenseRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int id, bool activate)
        {
            var recurring = await _repo.GetByIdAsync(id)
                ?? throw new InvalidDomainException($"Recurring expense {id} not found.");

            recurring.EnsureOwnership(userId);

            if (activate)
                recurring.Activate();
            else
                recurring.Deactivate();

            await _repo.UpdateAsync(recurring);
        }
    }

    /// <summary>
    /// Obtener todas las recurrencias del usuario.
    /// </summary>
    public class GetRecurringExpenseSummaryUseCase
    {
        private readonly IRecurringExpenseRepository _repo;
        private readonly RecurringExpenseGenerationService _genService;

        public GetRecurringExpenseSummaryUseCase(
            IRecurringExpenseRepository repo,
            RecurringExpenseGenerationService genService)
        {
            _repo = repo;
            _genService = genService;
        }

        public async Task<RecurringExpenseSummaryDto> ExecuteAsync(int userId)
        {
            var recurrences = await _repo.GetAllByUserAsync(userId);

            var dtos = recurrences.Select(r => new RecurringExpenseResponseDto
            {
                Id = r.Id,
                Description = r.Description,
                Amount = r.Amount.Value,
                Category = r.Category,
                Frequency = r.Frequency.ToString(),
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                LastGeneratedDate = r.LastGeneratedDate,
                NextOccurrence = r.GetNextOccurrence(),
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new RecurringExpenseSummaryDto
            {
                TotalRecurrences = dtos.Count,
                ActiveRecurrences = dtos.Count(d => d.IsActive),
                MonthlyProjectedTotal = await _genService.CalculateMonthlyProjectedTotalAsync(userId),
                Recurrences = dtos
            };
        }
    }

    /// <summary>
    /// Obtener una recurrencia por ID.
    /// </summary>
    public class GetRecurringExpenseByIdUseCase
    {
        private readonly IRecurringExpenseRepository _repo;

        public GetRecurringExpenseByIdUseCase(IRecurringExpenseRepository repo) => _repo = repo;

        public async Task<RecurringExpenseResponseDto?> ExecuteAsync(int userId, int id)
        {
            var recurring = await _repo.GetByIdAsync(id);
            if (recurring == null) return null;

            recurring.EnsureOwnership(userId);

            return new RecurringExpenseResponseDto
            {
                Id = recurring.Id,
                Description = recurring.Description,
                Amount = recurring.Amount.Value,
                Category = recurring.Category,
                Frequency = recurring.Frequency.ToString(),
                StartDate = recurring.StartDate,
                EndDate = recurring.EndDate,
                LastGeneratedDate = recurring.LastGeneratedDate,
                NextOccurrence = recurring.GetNextOccurrence(),
                IsActive = recurring.IsActive,
                CreatedAt = recurring.CreatedAt
            };
        }
    }
}