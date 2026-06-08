using Spendly.Application.DTOs.Tag;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;

namespace Spendly.Application.UseCases.Tags
{
    public class CreateTagUseCase
    {
        private readonly ITagRepository _repo;
        public CreateTagUseCase(ITagRepository repo) => _repo = repo;

        public async Task<int> ExecuteAsync(int userId, CreateTagDto dto)
        {
            // Prevent duplicates
            var existing = await _repo.GetByNameAsync(userId, dto.Name.Trim().ToLowerInvariant());
            if (existing != null)
                throw new InvalidOperationException($"Tag '{dto.Name}' already exists.");

            var tag = Tag.Create(userId, dto.Name, dto.Color);
            await _repo.AddAsync(tag);
            return tag.Id;
        }
    }

    public class UpdateTagUseCase
    {
        private readonly ITagRepository _repo;
        public UpdateTagUseCase(ITagRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int id, UpdateTagDto dto)
        {
            var tag = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Tag {id} not found.");
            tag.EnsureOwnership(userId);
            tag.Update(dto.Name, dto.Color);
            await _repo.UpdateAsync(tag);
        }
    }

    public class DeleteTagUseCase
    {
        private readonly ITagRepository _repo;
        public DeleteTagUseCase(ITagRepository repo) => _repo = repo;

        public async Task<bool> ExecuteAsync(int userId, int id)
        {
            var tag = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Tag {id} not found.");
            tag.EnsureOwnership(userId);
            return await _repo.DeleteAsync(id);
        }
    }

    public class ListTagsUseCase
    {
        private readonly ITagRepository _repo;
        public ListTagsUseCase(ITagRepository repo) => _repo = repo;

        public async Task<List<TagResponseDto>> ExecuteAsync(int userId)
        {
            var tags = await _repo.GetAllByUserAsync(userId);
            return tags.Select(t => new TagResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                ExpenseCount = t.ExpenseTags.Count(et => et.Expense != null && !et.Expense.IsDeleted),
                TotalSpent = t.ExpenseTags.Where(et => et.Expense != null && !et.Expense.IsDeleted).Sum(et => et.Expense.Amount.Value)
            }).ToList();
        }
    }

    public class SetExpenseTagsUseCase
    {
        private readonly ITagRepository _repo;
        public SetExpenseTagsUseCase(ITagRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int expenseId, List<int> tagIds)
        {
            await _repo.SetExpenseTagsAsync(userId, expenseId, tagIds);
        }
    }
}
