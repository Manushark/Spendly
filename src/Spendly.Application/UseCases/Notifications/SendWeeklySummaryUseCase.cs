using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Notifications
{
    /// <summary>
    /// Calcula las métricas de la semana pasada para un usuario y envía el digest por email.
    /// Solo envía si el usuario tiene EmailNotificationsEnabled y WeeklySummaryEmailEnabled.
    /// </summary>
    public class SendWeeklySummaryUseCase
    {
        private readonly IUserRepository _userRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly IEmailService _emailService;
        private readonly IDateTimeProvider _dateTime;

        public SendWeeklySummaryUseCase(
            IUserRepository userRepo,
            IExpenseRepository expenseRepo,
            IEmailService emailService,
            IDateTimeProvider dateTime)
        {
            _userRepo    = userRepo;
            _expenseRepo = expenseRepo;
            _emailService = emailService;
            _dateTime    = dateTime;
        }

        /// <summary>Sends a weekly digest to every user who opted in.</summary>
        public async Task ExecuteForAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();

            foreach (var user in users)
            {
                if (!user.EmailNotificationsEnabled || !user.WeeklySummaryEmailEnabled)
                    continue;

                await ExecuteForUserAsync(user.Id, user.Email, user.TimeZone);
            }
        }

        /// <summary>Sends a weekly digest for a single user (also used for testing).</summary>
        public async Task ExecuteForUserAsync(int userId, string toEmail, string timeZone = "UTC")
        {
            var now       = _dateTime.Now(timeZone);
            // Previous Monday → previous Sunday
            var lastMonday = now.Date.AddDays(-(int)now.DayOfWeek - 6);
            var lastSunday = lastMonday.AddDays(6);

            // Month-to-date (1st of current month → today)
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var weekTotal  = await _expenseRepo.GetTotalAmountAsync(userId, lastMonday, lastSunday);
            var monthTotal = await _expenseRepo.GetTotalAmountAsync(userId, monthStart, now.Date);

            var categoryTotals = await _expenseRepo.GetTotalByCategoryAsync(userId, lastMonday, lastSunday);
            var topCategory    = categoryTotals.Any()
                ? categoryTotals.OrderByDescending(kvp => kvp.Value).First().Key
                : "—";

            var txCount = await _expenseRepo.CountAsync(
                userId, null, null, lastMonday, lastSunday, null, null);

            await _emailService.SendWeeklySummaryEmailAsync(
                toEmail, weekTotal, monthTotal, topCategory, txCount);
        }
    }
}
