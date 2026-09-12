<div align="center">
  <h1>💸 Spendly</h1>
  <p><strong>A Modern, Secure & Full-Featured Personal Finance Management Platform</strong></p>

  <p>
    <a href="https://spendly-web-cncja8b2edephcd6.westus2-01.azurewebsites.net/" target="_blank">
      <img src="https://img.shields.io/badge/Live_Demo-Azure_App_Service-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white" alt="Live Demo on Azure" />
    </a>
  </p>

  <p>
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 8.0" />
    <img src="https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp" alt="C# 12" />
    <img src="https://img.shields.io/badge/Architecture-Clean_Architecture-2ea44f?style=flat-square" alt="Clean Architecture" />
    <img src="https://img.shields.io/badge/Tests-117%20Passed%20(100%25)-brightgreen?style=flat-square&logo=githubactions" alt="117 Tests Passed" />
    <img src="https://img.shields.io/badge/Database-SQL_Server_%26_EF_Core-CC292B?style=flat-square&logo=microsoftsqlserver" alt="SQL Server" />
    <img src="https://img.shields.io/badge/Workflow-GitFlow-orange?style=flat-square&logo=git" alt="GitFlow" />
    <img src="https://img.shields.io/badge/Release-v1.22.2-blue?style=flat-square" alt="Release v1.22.2" />
    <img src="https://img.shields.io/badge/License-Non--Commercial-blue?style=flat-square" alt="License" />
  </p>
</div>

<div align="center">
  <h3>🌐 <a href="https://spendly-web-cncja8b2edephcd6.westus2-01.azurewebsites.net/" target="_blank">Explore the Live Web Application</a></h3>
  <p>Deployed in production on Microsoft Azure App Service with automated cloud configurations.</p>
</div>

---

## 📖 Overview

**Spendly** is a production-ready personal finance platform built with **.NET 8** following strict **Clean Architecture** principles. It empowers users to take full control of their personal finances through real-time expense tracking, budget enforcement with automated email notifications, income logging, recurring payment management, visual savings goals, and advanced financial analytics.

Engineered with scalability, high test coverage (117 unit tests), enterprise security patterns, and a responsive mobile-first UI.

---

## ✨ Core Features & Modules

### 📊 Dashboard & Real-Time KPI Metrics
* **Smart Financial Overview:** Real-time calculation of Net Balance, Total Monthly Income, Total Monthly Expenses, and Active Budget allocations.
* **Mobile-First 2x2 KPI Grid (v1.22.2):** Compact dual-row grid layout optimized for mobile screens (<576px), ensuring immediate visual clarity without scrolling clutter.
* **Interactive Visual Analytics:** Integrated Chart.js charts displaying category-by-category expense breakdowns and monthly cash-flow trends.

### 🎯 Budgeting & Automated Alert System
* **Budget Limits & Tracking:** Set category-specific monthly spending caps.
* **Real-time Alert Engine:** Dynamic threshold detection alerting users at **80% (Warning)** and **100% (Exceeded)** of budget usage.
* **Automated Email Dispatch:** Integrates SMTP / MailKit to automatically dispatch budget warnings and digests directly to the user's inbox with configurable notification toggles.

### 🔄 Recurring Expenses & Subscriptions
* **Automated Recurring Charges:** Manage recurring payments (Netflix, Rent, Gym, Insurance) with flexible recurrence schedules (Daily, Weekly, Monthly, Yearly).
* **Next Due Date Calculator:** Intelligent due-date tracking with status indicators (Active, Due Soon, Overdue).

### 💰 Incomes & Savings Goals
* **Multi-Source Income Logging:** Categorize and track various revenue streams (Salary, Freelance, Investments).
* **Savings Goal Milestones:** Set target amounts and target dates with visual progress bars and percentage completion indicators.

### 📑 Reports & Financial Intelligence
* **Custom Range Analytics:** Filter transactions by specific dates, categories, and payment methods.
* **One-Click Export:** Seamless data export to **PDF** and **CSV** formats for accounting and tax preparation.

### 🎨 Theme Customizer & Modern UI/UX
* **Dual Theme Engine:** Instant toggle between Dark Mode and Light Mode with zero screen flicker.
* **Color Accent Selector:** Personalized palette customizer (Emerald, Indigo, Violet, Rose, Amber) utilizing dynamic CSS custom properties.
* **Glassmorphism Aesthetic:** Modern translucent cards, micro-interactions, responsive sidebars, and accessible touch targets.

### 🔒 Enterprise Security & Auth Flow
* **Robust Authentication:** Secure cookie-based authentication for Web MVC and JWT Bearer tokens for API endpoints.
* **Password Hashing:** Industry-standard BCrypt implementation with salted hashes.
* **Self-Service Password Reset:** End-to-end password recovery flow featuring secure, time-limited cryptographic tokens and branded HTML email templates.
* **Auditability & Soft Deletes:** Built-in soft-delete mechanisms with EF Core Global Query Filters and timestamp tracking (`CreatedAt`, `UpdatedAt`, `DeletedAt`).

---

## 🏗️ Architecture & Technology Stack

Spendly strictly adheres to the concentric layers of **Clean Architecture**, guaranteeing decoupled business rules, high testability, and framework independence.

```
                  ┌─────────────────────────────────────┐
                  │      Spendly.Web (ASP.NET MVC)      │
                  │      Spendly.Api (RESTful API)      │
                  └──────────────────┬──────────────────┘
                                     │
                  ┌──────────────────▼──────────────────┐
                  │    Spendly.Infrastructure (EF Core) │
                  │    - SQL Server DbContext           │
                  │    - MailKit SMTP Email Service     │
                  │    - Repository Implementations     │
                  └──────────────────┬──────────────────┘
                                     │
                  ┌──────────────────▼──────────────────┐
                  │    Spendly.Application (Use Cases)  │
                  │    - DTOs, Mappings & Interfaces    │
                  │    - BudgetAlertService & Engine    │
                  └──────────────────┬──────────────────┘
                                     │
                  ┌──────────────────▼──────────────────┐
                  │       Spendly.Domain (Core)         │
                  │    - Entities & Value Objects (Money)│
                  │    - Domain Exceptions & Rules      │
                  └─────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Project | Key Technologies & Responsibilities |
| :--- | :--- | :--- |
| **Domain** | `Spendly.Domain` | Enterprise core: `Expense`, `Budget`, `Income`, `SavingsGoal`, `RecurringExpense`, `User`, `Category`. Encapsulates `Money` Value Object, Domain Exceptions, and pure business invariants. Zero external dependencies. |
| **Application** | `Spendly.Application` | Business use cases, DTOs, repository abstractions (`IExpenseRepository`, `IBudgetRepository`), services (`BudgetAlertService`), and notification interfaces. |
| **Infrastructure** | `Spendly.Infrastructure` | Data access with **Entity Framework Core**, SQL Server migrations, repository implementations, SMTP email delivery via **MailKit**, and BCrypt password encryption. |
| **Web Presentation** | `Spendly.Web` | Client-facing **ASP.NET Core MVC** application with Razor Views, custom Vanilla CSS design tokens, Chart.js, responsive layouts, and cookie authentication. |
| **API Presentation** | `Spendly.Api` | RESTful API endpoints, JWT token issuance, Swagger documentation, and CORS configuration. |
| **Test Suite** | `Spendly.Tests` | Comprehensive unit and integration test suite (**117 passing tests**, 100% green) using **xUnit**, **Moq**, and **FluentAssertions**. |

---

## 🧪 Testing & Code Quality

* **117 Unit Tests:** Exhaustive coverage across Domain entities, invariants, Value Objects, Use Cases, and the automated Budget Alert Engine.
* **Zero Compiler Warnings:** Configured with `<Nullable>enable</Nullable>` adhering strictly to C# 12 nullable reference safety (0 warnings, 0 errors).
* **GitFlow Discipline:** Maintained with standard GitFlow branching (`main`, `develop`, `feature/*`, `fix/*`, `release/*`) and granular, single-responsibility commits.

To run the test suite locally:
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 🚀 Getting Started Locally

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Docker)
* Visual Studio 2022 / VS Code / JetBrains Rider

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Manushark/Spendly.git
   cd Spendly
   ```

2. **Configure Database & Connection String**
   * Edit `src/Spendly.Web/appsettings.Development.json` and `src/Spendly.Api/appsettings.Development.json` with your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SpendlyDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

3. **Apply EF Core Migrations**
   ```bash
   cd src/Spendly.Infrastructure
   dotnet ef database update --startup-project ../Spendly.Web
   ```

4. **Run the Application**
   ```bash
   # Option A: Run the Web MVC Application
   dotnet run --project src/Spendly.Web

   # Option B: Run the RESTful API
   dotnet run --project src/Spendly.Api
   ```
   Open your browser at `https://localhost:7194` (or the port displayed in your terminal).

---

## 🗺️ Versioning & Changelog

Spendly follows [Semantic Versioning](https://semver.org/). Recent milestones:
* **v1.22.2 (Current):** Mobile Compact KPI Grid 2x2 layout, compiler null-safety cleanup, automated tag synchronization.
* **v1.22.1:** Password Reset workflow with cryptographic tokens and MailKit SMTP integration.
* **v1.22.0:** Dynamic Theme Customizer (Dark/Light mode & custom accent colors).
* **v1.21.0:** Advanced Reports module with PDF and CSV export capabilities.

See [CHANGELOG.md](CHANGELOG.md) for full historical details.

---

## ⚖️ License

**Copyright (c) 2026 Manuel Rivas. All rights reserved.**

This project is released under a custom **Non-Commercial Educational License**. You are welcome to review, test, and study the code for evaluation, personal, and educational purposes. Commercial redistribution or monetization without prior explicit permission is prohibited.

---

## 👨‍💻 Author

**Manuel Rivas**  
*Full-Stack Software Developer | .NET & Clean Architecture Enthusiast*  
* [GitHub Profile](https://github.com/Manushark)  
* [Live Project Demo](https://spendly-web-cncja8b2edephcd6.westus2-01.azurewebsites.net/)
