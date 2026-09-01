using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.User
{
    public class UpdateNotificationPreferencesDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool BudgetAlertEmailEnabled { get; set; }
        public bool WeeklySummaryEmailEnabled { get; set; }
    }

    public class GetNotificationPreferencesDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool BudgetAlertEmailEnabled { get; set; }
        public bool WeeklySummaryEmailEnabled { get; set; }
    }

    public class UpdateNotificationPreferencesUseCase
    {
        private readonly IUserRepository _userRepo;

        public UpdateNotificationPreferencesUseCase(IUserRepository userRepo)
            => _userRepo = userRepo;

        public async Task ExecuteAsync(int userId, UpdateNotificationPreferencesDto dto)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new Domain.Exceptions.InvalidDomainException("User not found.");

            user.UpdateNotificationPreferences(
                dto.EmailNotificationsEnabled,
                dto.BudgetAlertEmailEnabled,
                dto.WeeklySummaryEmailEnabled);

            await _userRepo.UpdateAsync(user);
        }
    }

    public class GetNotificationPreferencesUseCase
    {
        private readonly IUserRepository _userRepo;

        public GetNotificationPreferencesUseCase(IUserRepository userRepo)
            => _userRepo = userRepo;

        public async Task<GetNotificationPreferencesDto> ExecuteAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new Domain.Exceptions.InvalidDomainException("User not found.");

            return new GetNotificationPreferencesDto
            {
                EmailNotificationsEnabled = user.EmailNotificationsEnabled,
                BudgetAlertEmailEnabled   = user.BudgetAlertEmailEnabled,
                WeeklySummaryEmailEnabled  = user.WeeklySummaryEmailEnabled
            };
        }
    }
}
