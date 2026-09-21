using Spendly.Application.DTOs.Ai;
using Spendly.Application.Interfaces;
using Spendly.Domain.Exceptions;

namespace Spendly.Application.UseCases.Ai
{
    public class ParseAiCommandUseCase
    {
        private readonly IAiAssistantService _aiService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ParseAiCommandUseCase(
            IAiAssistantService aiService,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IDateTimeProvider dateTimeProvider)
        {
            _aiService = aiService;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<AiFinancialPlanDto> ExecuteAsync(int userId, string prompt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new InvalidDomainException("Voice or text command cannot be empty.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var timeZone = user.TimeZone ?? "UTC";
            var referenceDate = _dateTimeProvider.Now(timeZone);

            var categories = await _categoryRepository.GetAllByUserAsync(userId);
            var categoryNames = categories.Select(c => c.Name).ToList();

            if (!categoryNames.Any())
            {
                categoryNames = ["Food & Dining", "Transportation", "Entertainment", "Shopping", "Bills & Utilities", "Health", "Other"];
            }

            var plan = await _aiService.ParseCommandAsync(
                prompt,
                referenceDate,
                timeZone,
                categoryNames,
                cancellationToken);

            plan.AvailableCategories = categoryNames;
            return plan;
        }
    }
}
