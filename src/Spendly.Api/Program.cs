using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Spendly.Infrastructure.Persistence;
using Spendly.Infrastructure.Repositories;
using Spendly.Infrastructure.Security;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCase.ListExpenses;
using Spendly.Application.UseCase.GetExpenseById;
using Spendly.Application.UseCase.DeleteExpense;
using Spendly.Application.UseCases.Expenses;
using Spendly.Application.UseCases.Auth;
using Spendly.Api.Middlewares;
using Spendly.Application.UseCase.Dashboard;
using Spendly.Application.UseCases.Budgets;
using Spendly.Application.Services;
using Spendly.Application.UseCases.RecurringExpenses;
using Spendly.Infrastructure.Jobs;
using Spendly.Application.UseCases.User;
using Spendly.Application.UseCases.Categories;
using Spendly.Application.UseCases.Incomes;
using Spendly.Application.UseCases.Notifications;
using Spendly.Application.UseCases.Exports;
using Spendly.Application.UseCases.Insights;
using Spendly.Application.UseCases.SavingsGoals;
using Spendly.Application.UseCases.Tags;
using Spendly.Application.UseCases.Import;
using Spendly.Application.UseCase.Reports;
using Spendly.Api.Security;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// ────────────────────────────────────────────────────────────────
// Configuration: Environment variables override appsettings.json
// Priority: appsettings.json < appsettings.{Env}.json < env vars
// ────────────────────────────────────────────────────────────────

var configuration = builder.Configuration;
var isDevelopment = builder.Environment.IsDevelopment();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ────────────────────────────────────────────────────────────
// CORS Configuration
// ────────────────────────────────────────────────────────────
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://localhost:7100", "http://localhost:5010" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpendlyPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();
        }

        var response = JsonSerializer.Serialize(new
        {
            status = StatusCodes.Status429TooManyRequests,
            error = "Too many requests. Please slow down and try again shortly."
        });

        await context.HttpContext.Response.WriteAsync(response, token);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"global:{RateLimitPolicies.GetPartitionKey(httpContext)}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"auth:{RateLimitPolicies.GetPartitionKey(httpContext)}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(RateLimitPolicies.WriteOperations, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"write:{RateLimitPolicies.GetPartitionKey(httpContext)}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(RateLimitPolicies.ImportPreview, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"import-preview:{RateLimitPolicies.GetPartitionKey(httpContext)}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 6,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(RateLimitPolicies.ImportConfirm, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"import-confirm:{RateLimitPolicies.GetPartitionKey(httpContext)}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// ────────────────────────────────────────────────────────────
// Health Checks
// ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SpendlyDbContext>("database");

// ────────────────────────────────────────────────────────────
// Database Configuration
// ────────────────────────────────────────────────────────────
var connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is not configured. " +
        "Set 'ConnectionStrings__DefaultConnection' environment variable " +
        "or configure it in appsettings.{Environment}.json.");
}

builder.Services.AddDbContext<SpendlyDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Retry on transient failures (Azure SQL wake-up from auto-pause, network blips)
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        // Give Azure SQL enough time to wake from auto-pause
        sqlOptions.CommandTimeout(60);
    }));

// ────────────────────────────────────────────────────────────
// JWT Configuration
// ────────────────────────────────────────────────────────────
var jwtKey = configuration["Jwt:Key"];
var jwtIssuer = configuration["Jwt:Issuer"] ?? "Spendly";
var jwtAudience = configuration["Jwt:Audience"] ?? "SpendlyUsers";
var jwtExpirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var expMin) ? expMin : 120;

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT signing key is not configured. " +
        "Set 'Jwt__Key' environment variable " +
        "or configure it in appsettings.{Environment}.json.");
}

if (!isDevelopment && jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT key must be at least 32 characters in production. " +
        "Set a strong key via 'Jwt__Key' environment variable.");
}

builder.Services.AddSingleton<IJwtTokenGenerator>(
    new JwtTokenGenerator(jwtKey, jwtIssuer, jwtAudience, jwtExpirationMinutes));

// ────────────────────────────────────────────────────────────
// Authentication
// ────────────────────────────────────────────────────────────
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = !isDevelopment,
        ValidIssuer = jwtIssuer,
        ValidateAudience = !isDevelopment,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// ────────────────────────────────────────────────────────────
// Repositories
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IRecurringExpenseRepository, RecurringExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// ────────────────────────────────────────────────────────────
// Use Cases — Expenses
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<CreateExpenseUseCase>();
builder.Services.AddScoped<ListExpensesUseCase>();
builder.Services.AddScoped<GetExpenseByIdUseCase>();
builder.Services.AddScoped<DeleteExpenseUseCase>();
builder.Services.AddScoped<UpdateExpenseUseCase>();
builder.Services.AddScoped<ExportExpensesCsvUseCase>();
builder.Services.AddScoped<ExportMonthlyReportUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Budgets
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<CreateBudgetUseCase>();
builder.Services.AddScoped<UpdateBudgetUseCase>();
builder.Services.AddScoped<DeleteBudgetUseCase>();
builder.Services.AddScoped<GetBudgetByIdUseCase>();
builder.Services.AddScoped<GetBudgetSummaryUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Dashboard
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<GetDashboardStatsUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Reports
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<GetFinancialReportUseCase>();


// ────────────────────────────────────────────────────────────
// Use Cases — Auth
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RegisterUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — User Profile
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<GetUserProfileUseCase>();
builder.Services.AddScoped<UpdateUserProfileUseCase>();
builder.Services.AddScoped<ChangePasswordUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Categories
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<GetCategoriesUseCase>();
builder.Services.AddScoped<CreateCategoryUseCase>();
builder.Services.AddScoped<UpdateCategoryUseCase>();
builder.Services.AddScoped<DeleteCategoryUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Incomes
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<CreateIncomeUseCase>();
builder.Services.AddScoped<UpdateIncomeUseCase>();
builder.Services.AddScoped<DeleteIncomeUseCase>();
builder.Services.AddScoped<ListIncomesUseCase>();
builder.Services.AddScoped<GetIncomeByIdUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Notifications & Budget Alerts
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<GetNotificationsUseCase>();
builder.Services.AddScoped<MarkNotificationReadUseCase>();
builder.Services.AddScoped<MarkAllNotificationsReadUseCase>();
builder.Services.AddScoped<GetUnreadCountUseCase>();
builder.Services.AddSingleton<IDateTimeProvider, UserDateTimeProvider>();
builder.Services.AddScoped<BudgetAlertService>();

// ────────────────────────────────────────────────────────────
// Use Cases — Recurring Expenses
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<RecurringExpenseGenerationService>();
builder.Services.AddScoped<CreateRecurringExpenseUseCase>();
builder.Services.AddScoped<UpdateRecurringExpenseUseCase>();
builder.Services.AddScoped<DeleteRecurringExpenseUseCase>();
builder.Services.AddScoped<ToggleRecurringExpenseUseCase>();
builder.Services.AddScoped<GetRecurringExpenseSummaryUseCase>();
builder.Services.AddScoped<GetRecurringExpenseByIdUseCase>();
builder.Services.AddHostedService<RecurringExpenseBackgroundService>();
builder.Services.AddHostedService<DatabaseMigrationService>();

// ────────────────────────────────────────────────────────────
// Use Cases — Insights
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<GetMonthlyInsightsUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Savings Goals
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<ISavingsGoalRepository, SavingsGoalRepository>();
builder.Services.AddScoped<CreateSavingsGoalUseCase>();
builder.Services.AddScoped<UpdateSavingsGoalUseCase>();
builder.Services.AddScoped<DeleteSavingsGoalUseCase>();
builder.Services.AddScoped<AddFundsUseCase>();
builder.Services.AddScoped<ListSavingsGoalsUseCase>();
builder.Services.AddScoped<GetSavingsGoalByIdUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Tags
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<CreateTagUseCase>();
builder.Services.AddScoped<UpdateTagUseCase>();
builder.Services.AddScoped<DeleteTagUseCase>();
builder.Services.AddScoped<ListTagsUseCase>();
builder.Services.AddScoped<SetExpenseTagsUseCase>();

// ────────────────────────────────────────────────────────────
// Use Cases — Import
// ────────────────────────────────────────────────────────────
builder.Services.AddScoped<ImportCsvUseCase>();

// ════════════════════════════════════════════════════════════
// Pipeline
// ════════════════════════════════════════════════════════════
var app = builder.Build();

// Database migrations and startup fixes are handled asynchronously in the background by DatabaseMigrationService.

if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (!isDevelopment)
{
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("SpendlyPolicy");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

// ────────────────────────────────────────────────────────────
// Lightweight ping (NO database) — used by warmup service
// ────────────────────────────────────────────────────────────
app.MapGet("/api/ping", () => Results.Ok(new { status = "alive", timestamp = DateTime.UtcNow }));

// ────────────────────────────────────────────────────────────
// Health Check Endpoint
// ────────────────────────────────────────────────────────────
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();
