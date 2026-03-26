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

using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Spendly.Infrastructure.Services;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - Allow Web frontend to call this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:7024", "http://localhost:5115", "http://localhost:5242")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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

// register user secrets for development
builder.Configuration.AddUserSecrets<Program>();

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

#region Authentication & Google OAuth

// ✅ AUTENTICACIÓN - JWT + Google OAuth (opcional)
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    var jwtKeyValue = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT Key not configured");

    options.TokenValidationParameters = new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyValue))
    };
});

// Google OAuth - solo si las credenciales están configuradas
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;

        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.SaveTokens = true;

        options.ClaimActions.MapJsonKey("picture", "picture");
        options.ClaimActions.MapJsonKey("verified_email", "verified_email");

        options.Events.OnCreatingTicket = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Google user authenticated: {Email}",
                context.Principal?.FindFirstValue(ClaimTypes.Email));

            return Task.CompletedTask;
        };
    });
}

// Always register GoogleAuthService so DI doesn't crash 
// (controller checks if Google is configured before using it)
builder.Services.AddScoped<GoogleAuthService>();

#endregion

// ✅ ELIMINADO: La segunda llamada a AddAuthentication que causaba el error

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

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
