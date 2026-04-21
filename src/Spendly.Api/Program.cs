using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

var builder = WebApplication.CreateBuilder(args);

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
    options.UseSqlServer(connectionString));

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

if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
