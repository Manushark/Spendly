using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Spendly.Infrastructure.Persistence
{
    public class DatabaseMigrationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseMigrationService> _logger;

        public DatabaseMigrationService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Yield immediately to let the API start and serve requests (like /api/ping)
            await Task.Yield();

            _logger.LogInformation("DatabaseMigrationService: Starting background database migration check...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SpendlyDbContext>();

                _logger.LogInformation("DatabaseMigrationService: Applying migrations (this may take a while if Azure SQL is waking up)...");
                await db.Database.MigrateAsync(stoppingToken);
                _logger.LogInformation("DatabaseMigrationService: Migrations applied successfully.");

                // Fix default user registration dates
                var usersToFix = await db.Users.Where(u => u.CreatedAt.Year <= 2000).ToListAsync(stoppingToken);
                if (usersToFix.Any())
                {
                    _logger.LogInformation("DatabaseMigrationService: Fixing {Count} users with default/invalid CreatedAt dates...", usersToFix.Count);
                    foreach (var u in usersToFix)
                    {
                        var prop = typeof(Spendly.Domain.Entities.User).GetProperty("CreatedAt");
                        prop?.SetValue(u, new DateTime(2026, 5, 20, 12, 0, 0));
                    }
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("DatabaseMigrationService: Users fixed successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DatabaseMigrationService: Failed to apply database migrations or fix users in background.");
            }
        }
    }
}
