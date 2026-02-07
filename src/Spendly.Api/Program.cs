using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Add this using directive for JwtBearer authentication
using Microsoft.IdentityModel.Tokens; 
using System.Text; 
using Spendly.Infrastructure.Persistence;
using Spendly.Infrastructure.Repositories;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCase.ListExpenses;
using Spendly.Application.UseCase.GetExpenseById;
using Spendly.Application.UseCase.DeleteExpense;
using Spendly.Application.UseCases.Expenses;
using Spendly.Api.Middlewares;



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



// Use Cases
builder.Services.AddScoped<CreateExpenseUseCase>();

// List Expenses Use Case
builder.Services.AddScoped<ListExpensesUseCase>();

// Get Expense By Id Use Case
builder.Services.AddScoped<GetExpenseByIdUseCase>();

// Delete Expense Use Case
builder.Services.AddScoped<DeleteExpenseUseCase>();

// Update Expense Use Case
builder.Services.AddScoped<UpdateExpenseUseCase>();

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("SUPER_SECRET_KEY_123")
            )
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

app.UseAuthorization();

app.MapControllers();

app.Run();
