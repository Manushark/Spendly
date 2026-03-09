using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spendly.Application.Services;

namespace Spendly.Infrastructure.Jobs
{
    /// <summary>
    /// Servicio en background que ejecuta la generación de gastos recurrentes cada día.
    /// </summary>
    public class RecurringExpenseBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringExpenseBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);  // Revisar cada hora

        public RecurringExpenseBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RecurringExpenseBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Expense Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateRecurringExpenses();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating recurring expenses.");
                }

                // Esperar 1 hora antes de la próxima verificación
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Recurring Expense Background Service stopped.");
        }

        private async Task GenerateRecurringExpenses()
        {
            using var scope = _serviceProvider.CreateScope();
            var generationService = scope.ServiceProvider
                .GetRequiredService<RecurringExpenseGenerationService>();

            var generatedCount = await Task.Run(() => generationService.GeneratePendingExpenses());

            if (generatedCount > 0)
            {
                _logger.LogInformation(
                    "Generated {Count} recurring expenses at {Time}",
                    generatedCount,
                    DateTime.Now);
            }
        }
    }
}
