using Spendly.Application.DTOs.SavingsGoal;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;

namespace Spendly.Application.UseCases.SavingsGoals
{
    public class CreateSavingsGoalUseCase
    {
        private readonly ISavingsGoalRepository _repo;
        public CreateSavingsGoalUseCase(ISavingsGoalRepository repo) => _repo = repo;

        public async Task<int> ExecuteAsync(int userId, CreateSavingsGoalDto dto)
        {
            var goal = SavingsGoal.Create(userId, dto.Name, dto.TargetAmount, dto.CurrentAmount, dto.Deadline, dto.Icon, dto.Color);
            await _repo.AddAsync(goal);
            return goal.Id;
        }
    }

    public class UpdateSavingsGoalUseCase
    {
        private readonly ISavingsGoalRepository _repo;
        public UpdateSavingsGoalUseCase(ISavingsGoalRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int id, UpdateSavingsGoalDto dto)
        {
            var goal = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Savings goal {id} not found.");
            goal.EnsureOwnership(userId);
            goal.Update(dto.Name, dto.TargetAmount, dto.CurrentAmount, dto.Deadline, dto.Icon, dto.Color);
            await _repo.UpdateAsync(goal);
        }
    }

    public class DeleteSavingsGoalUseCase
    {
        private readonly ISavingsGoalRepository _repo;
        public DeleteSavingsGoalUseCase(ISavingsGoalRepository repo) => _repo = repo;

        public async Task<bool> ExecuteAsync(int userId, int id)
        {
            var goal = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Savings goal {id} not found.");
            goal.EnsureOwnership(userId);
            return await _repo.DeleteAsync(id);
        }
    }

    public class AddFundsUseCase
    {
        private readonly ISavingsGoalRepository _repo;
        public AddFundsUseCase(ISavingsGoalRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int id, decimal amount)
        {
            var goal = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Savings goal {id} not found.");
            goal.EnsureOwnership(userId);
            goal.AddFunds(amount);
            await _repo.UpdateAsync(goal);
        }
    }

    public class ListSavingsGoalsUseCase
    {
        private readonly ISavingsGoalRepository _repo;
        public ListSavingsGoalsUseCase(ISavingsGoalRepository repo) => _repo = repo;

        public async Task<List<SavingsGoalResponseDto>> ExecuteAsync(int userId)
        {
            var goals = await _repo.GetAllByUserAsync(userId);
            return goals.Select(g => new SavingsGoalResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.CurrentAmount,
                ProgressPercentage = g.ProgressPercentage,
                Deadline = g.Deadline,
                Icon = g.Icon,
                Color = g.Color,
                IsCompleted = g.IsCompleted,
                CreatedAt = g.CreatedAt
            }).ToList();
        }
    }

    public class GetSavingsGoalByIdUseCase
    {
        private readonly ISavingsGoalRepository _repo;
        public GetSavingsGoalByIdUseCase(ISavingsGoalRepository repo) => _repo = repo;

        public async Task<SavingsGoalResponseDto?> ExecuteAsync(int userId, int id)
        {
            var g = await _repo.GetByIdAsync(id);
            if (g == null) return null;
            g.EnsureOwnership(userId);
            return new SavingsGoalResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.CurrentAmount,
                ProgressPercentage = g.ProgressPercentage,
                Deadline = g.Deadline,
                Icon = g.Icon,
                Color = g.Color,
                IsCompleted = g.IsCompleted,
                CreatedAt = g.CreatedAt
            };
        }
    }
}
