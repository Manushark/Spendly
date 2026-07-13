# Spendly — Architecture

## Overview

Spendly follows **Clean Architecture** with a clear separation between Domain, Application, Infrastructure, and Presentation layers. No layer depends on a layer above it.

```
┌──────────────────────────────────────────────────────────────────┐
│                        PRESENTATION                              │
│   Spendly.Web (MVC)         │   Spendly.Api (REST JSON API)      │
│   Controllers, Views,       │   Controllers, DTOs, JWT Auth      │
│   Razor Pages, Clients      │   Rate Limiting, Health Checks     │
└──────────────────┬──────────────────────┬────────────────────────┘
                   │                      │
┌──────────────────▼──────────────────────▼────────────────────────┐
│                       APPLICATION                                │
│   Spendly.Application                                            │
│   Use Cases (one class per action)                               │
│   DTOs (request/response shapes)                                 │
│   Interfaces (contracts for infrastructure)                      │
│   Services (BudgetAlertService, RecurringExpenseGenerationService)│
└──────────────────┬───────────────────────────────────────────────┘
                   │
┌──────────────────▼───────────────────────────────────────────────┐
│                         DOMAIN                                   │
│   Spendly.Domain                                                 │
│   Entities (User, Expense, Budget, Income, SavingsGoal, ...)     │
│   Value Objects (Money)                                          │
│   Exceptions (InvalidDomainException, InvalidCredentialsException)│
│   Enums (NotificationType, RecurrenceFrequency)                  │
└──────────────────┬───────────────────────────────────────────────┘
                   │
┌──────────────────▼───────────────────────────────────────────────┐
│                     INFRASTRUCTURE                               │
│   Spendly.Infrastructure                                         │
│   Repositories (EF Core implementations of interfaces)          │
│   SpendlyDbContext (SQL Server via Entity Framework Core)        │
│   DateTimeProvider, JwtTokenGenerator, BcryptPasswordHasher      │
└──────────────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
Spendly/
├── src/
│   ├── Spendly.Domain/           # Pure business logic, no dependencies
│   │   ├── Entities/             # User, Expense, Budget, Income, etc.
│   │   ├── Exceptions/           # Domain-specific exceptions
│   │   ├── Enums/                # NotificationType, RecurrenceFrequency
│   │   └── ValueObjects/         # Money
│   │
│   ├── Spendly.Application/      # Use cases and interfaces
│   │   ├── UseCase/              # One folder per domain (Expenses, Budgets, ...)
│   │   ├── Services/             # BudgetAlertService, RecurringExpenseGenerationService
│   │   ├── Interfaces/           # IExpenseRepository, IJwtTokenGenerator, etc.
│   │   └── DTOs/                 # Request/response models
│   │
│   ├── Spendly.Infrastructure/   # EF Core, SQL Server, external services
│   │   ├── Repositories/         # Implements all IRepository interfaces
│   │   ├── Persistence/          # SpendlyDbContext, Migrations
│   │   └── Services/             # JwtTokenGenerator, DateTimeProvider
│   │
│   ├── Spendly.Api/              # REST API (JSON)
│   │   ├── Controllers/          # 14 controllers
│   │   ├── Security/             # JWT config, rate limiting policies
│   │   └── Extensions/           # ClaimsPrincipal helpers
│   │
│   └── Spendly.Web/              # MVC Web App (Razor Views)
│       ├── Controllers/          # Web controllers
│       ├── Views/                # Razor views per module
│       ├── Services/             # ApiClient services (call Spendly.Api)
│       └── Resources/            # i18n files (EN/ES)
│
├── Spendly.Tests/                # xUnit unit tests
│   └── UseCases/                 # Tests per domain module
│
└── docs/                         # This documentation
```

---

## Key Design Decisions

### 1. Use Case Pattern
Each user action is a dedicated class with a single `ExecuteAsync` method. This makes the code:
- Easy to test (each class has exactly one responsibility)
- Easy to find (one file = one feature)
- Easy to extend (add a new use case without touching existing ones)

### 2. Repository Pattern
All data access goes through interfaces (`IExpenseRepository`, `IBudgetRepository`, etc.) defined in `Spendly.Application`. The concrete implementations live in `Spendly.Infrastructure`. This allows:
- Swapping databases without touching business logic
- Mocking repositories in unit tests (no database needed)

### 3. Dual Frontend
- **`Spendly.Api`** — stateless REST API, consumed by the MVC app and potentially mobile apps
- **`Spendly.Web`** — MVC app that calls the API via typed `ApiClient` services

### 4. Domain Validation
Validation lives in Domain entities, not in controllers or DTOs. For example:
```csharp
// In Income.cs (Domain) — always valid, regardless of who creates it
if (amount <= 0)
    throw new InvalidDomainException("Amount must be greater than zero.");
```

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 12 / .NET 8 |
| ORM | Entity Framework Core 9 |
| Database | SQL Server (Azure SQL) |
| Auth | JWT Bearer Tokens + BCrypt |
| Testing | xUnit + Moq |
| CI/CD | GitHub Actions → Azure App Service |
| Frontend Charts | ApexCharts, Chart.js |
| i18n | ASP.NET Core Localization (EN/ES) |
| Background Jobs | IHostedService (recurring expenses) |

---

## Data Flow Example: Create Expense

```
Browser (POST /api/expenses)
  → ExpensesController.Create()
    → CreateExpenseUseCase.ExecuteAsync()
      → Expense.Create()           [Domain validation]
      → IExpenseRepository.AddAsync()
        → ExpenseRepository (EF Core)
          → SQL Server
```
