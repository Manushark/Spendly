using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spendly.Application.UseCases.Notifications;

namespace Spendly.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Hosted service that fires every Monday at 08:00 UTC
    /// and sends the weekly spending digest to all opted-in users.
    /// </summary>
    public class WeeklySummaryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeeklySummaryBackgroundService> _logger;

        public WeeklySummaryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<WeeklySummaryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[WeeklySummary] Background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextMonday();
                _logger.LogInformation("[WeeklySummary] Next run in {Hours:N1} hours.", delay.TotalHours);

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                await SendDigestAsync();
            }
        }

        private async Task SendDigestAsync()
        {
            _logger.LogInformation("[WeeklySummary] Sending weekly digest to all opted-in users...");
            try
            {
                using var scope   = _scopeFactory.CreateScope();
                var useCase       = scope.ServiceProvider.GetRequiredService<SendWeeklySummaryUseCase>();
                await useCase.ExecuteForAllUsersAsync();
                _logger.LogInformation("[WeeklySummary] Weekly digest completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WeeklySummary] Error while sending weekly digest.");
            }
        }

        /// <summary>
        /// Calculates the delay until next Monday at 08:00 UTC.
        /// If today IS Monday and it's before 08:00, fires today; otherwise next Monday.
        /// </summary>
        private static TimeSpan GetDelayUntilNextMonday()
        {
            var now     = DateTime.UtcNow;
            var nextRun = now.Date;

            // Advance to next Monday
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && now.TimeOfDay >= TimeSpan.FromHours(8))
                daysUntilMonday = 7; // already past today's window → next Monday

            nextRun = nextRun.AddDays(daysUntilMonday).AddHours(8); // 08:00 UTC

            return nextRun - now;
        }
    }
}
