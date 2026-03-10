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



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  Database connection
builder.Services.AddDbContext<SpendlyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();

#region Expenses

// User Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
builder.Services.AddSingleton<IJwtTokenGenerator>(new JwtTokenGenerator(jwtKey));

// Use Cases - Expenses
builder.Services.AddScoped<CreateExpenseUseCase>();

// List Expenses Use Case
builder.Services.AddScoped<ListExpensesUseCase>();

// Get Expense By Id Use Case
builder.Services.AddScoped<GetExpenseByIdUseCase>();

// Delete Expense Use Case
builder.Services.AddScoped<DeleteExpenseUseCase>();

// Update Expense Use Case
builder.Services.AddScoped<UpdateExpenseUseCase>();
#endregion

#region Budgets
// Budget Repository
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<CreateBudgetUseCase>();
builder.Services.AddScoped<UpdateBudgetUseCase>();
builder.Services.AddScoped<DeleteBudgetUseCase>();
builder.Services.AddScoped<GetBudgetByIdUseCase>();
builder.Services.AddScoped<GetBudgetSummaryUseCase>();
#endregion 

// Dashboard use cases
builder.Services.AddScoped<GetDashboardStatsUseCase>();

// Use Cases - Auth
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RegisterUseCase>();

#region Recurring Expenses
// Recurrent Expenses
builder.Services.AddScoped<IRecurringExpenseRepository, RecurringExpenseRepository>();

// Recurring Expense Generation Service
builder.Services.AddScoped<RecurringExpenseGenerationService>();

// Use Cases - Recurring Expenses
builder.Services.AddScoped<CreateRecurringExpenseUseCase>();
builder.Services.AddScoped<UpdateRecurringExpenseUseCase>();
builder.Services.AddScoped<DeleteRecurringExpenseUseCase>();
builder.Services.AddScoped<ToggleRecurringExpenseUseCase>();
builder.Services.AddScoped<GetRecurringExpenseSummaryUseCase>();
builder.Services.AddScoped<GetRecurringExpenseByIdUseCase>();

// Background Service for Recurring Expense Generation
builder.Services.AddHostedService<RecurringExpenseBackgroundService>();
#endregion

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Ensure these namespaces are included at the top of your file
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
