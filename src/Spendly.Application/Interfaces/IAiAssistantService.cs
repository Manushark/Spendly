using Spendly.Application.DTOs.Ai;

namespace Spendly.Application.Interfaces
{
    public interface IAiAssistantService
    {
        Task<AiFinancialPlanDto> ParseCommandAsync(
            string prompt,
            DateTime referenceDate,
            string timeZone,
            IEnumerable<string> validCategories,
            CancellationToken cancellationToken = default);
    }
}
