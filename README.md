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
    <img src="https://img.shields.io/badge/Tests-124%20Passed%20(100%25)-brightgreen?style=flat-square&logo=githubactions" alt="124 Tests Passed" />
    <img src="https://img.shields.io/badge/Database-PostgreSQL_%26_Supabase-336791?style=flat-square&logo=postgresql" alt="PostgreSQL Supabase" />
    <img src="https://img.shields.io/badge/AI-Google_Gemini-8E75C4?style=flat-square&logo=google" alt="Google Gemini AI" />
    <img src="https://img.shields.io/badge/Workflow-GitFlow-orange?style=flat-square&logo=git" alt="GitFlow" />
    <img src="https://img.shields.io/badge/Release-v1.27.0-blue?style=flat-square" alt="Release v1.27.0" />
    <img src="https://img.shields.io/badge/License-Non--Commercial-blue?style=flat-square" alt="License" />
  </p>
</div>

<div align="center">
  <h3>🌐 <a href="https://spendly-web-cncja8b2edephcd6.westus2-01.azurewebsites.net/" target="_blank">Explore the Live Web Application</a></h3>
  <p>Deployed in production on Microsoft Azure App Service and powered by Supabase PostgreSQL.</p>
</div>

---

## 📖 Overview

**Spendly** is a production-ready personal finance platform built with **.NET 8** following strict **Clean Architecture** principles. It empowers users to take full control of their financial life through real-time expense tracking, automated budget threshold enforcement with email notifications, income logging, recurring subscription management, visual savings goals, advanced financial analytics, and an intelligent **AI Copilot** powered by **Google Gemini**.

Engineered for reliability and speed with **PostgreSQL on Supabase**, zero cold starts, 100% test coverage (**124 automated unit and integration tests**), enterprise security patterns, and a responsive mobile-first UI with full bilingual support (**English & Spanish**).

---

## ✨ Core Features & Modules

### 🤖 Spendly AI Copilot (Voice & Gemini AI)
* **Natural Language Financial Assistant:** Register expenses, incomes, or category budgets simply by speaking or typing naturally (e.g., *"Gasté 45 dólares en combustible hoy"* or *"Set a $400 budget for Groceries"*).
* **Multi-Model Resilient Fallback Chain:** Architecture using Google Gemini (`gemini-3-flash-preview` $\rightarrow$ `gemini-3.5-flash-lite` $\rightarrow$ `gemini-flash-lite-latest`) guaranteeing sub-second response times and zero downtime even under high Google API load.
* **Integrated Native Speech-to-Text:** Built-in microphone recognition with pulse animations, live transcript preview, and automatic audio cleanup upon dismissal.

### 📊 Dashboard & Dual-Mode Visual Analytics
* **Smart Financial Overview:** Real-time calculation of Net Balance, Total Monthly Income, Total Monthly Expenses, and Active Budget allocations.
* **Dual-Chart Toggle Mode:** Interactive pill switcher allowing users to alternate instantly without page reloads between **Monthly Comparison Bars** and **30-Day Daily Trend Lines**.
* **Mobile-First 2x2 KPI Grid:** Compact dual-row metric layout optimized for mobile screens (<576px) ensuring immediate financial clarity.

### 🎯 Budgeting & Automated Alert System
* **Budget Limits & Tracking:** Set category-specific monthly spending caps.
* **Real-time Alert Engine:** Dynamic threshold detection alerting users at **80% (Warning)** and **100% (Exceeded)** of budget usage.
* **Automated Email Dispatch:** Integrates SMTP / MailKit to automatically dispatch budget warnings and weekly financial digests directly to the user's inbox with configurable notification preferences.

### 🔄 Recurring Expenses & Subscriptions
* **Automated Recurring Charges:** Manage recurring payments (Netflix, Rent, Gym, Insurance) with flexible recurrence schedules (Daily, Weekly, Monthly, Yearly).
* **Next Due Date Calculator:** Intelligent due-date tracking with status indicators (Active, Due Soon, Overdue).

### 💰 Incomes & Savings Goals
* **Multi-Source Income Logging:** Categorize and track various revenue streams (Salary, Freelance, Investments).
* **Savings Goal Milestones:** Set target amounts and target dates with visual progress bars and percentage completion indicators.

### 📑 Reports & Financial Intelligence
* **Custom Range Analytics:** Filter transactions by specific dates, categories, tags, and amounts.
* **One-Click Export:** Seamless data export to **PDF** (via QuestPDF) and **CSV** formats for accounting and tax preparation.
* **Insights & Trend Forecasting:** End-of-month projection algorithms comparing current spending velocity against historical averages.

### 🌍 Internationalization (i18n)
* **Native Bilingual Architecture:** Instant switching between **Spanish** and **English** across all views, dashboards, chart series, badges, and automated email alerts.

### 🎨 Midnight Slate & Glassmorphic UI/UX
* **Dual Theme Engine:** Instant toggle between Dark Mode and Light Mode with zero screen flicker.
* **Midnight Slate Design System:** Deep dark background with subtle mesh patterns, ambient glow circles, glowing badges, and smooth micro-interactions.
* **Animated Thought Cloud:** Animated dream-style thought cloud (`.thought-cloud`) in the password recovery flow prompting users to check their Spam/Junk folder.

### 🔒 Enterprise Security & Database
* **PostgreSQL on Supabase:** High-performance database connected via transactional connection pooling (`aws-0-us-east-1.pooler.supabase.com`), universal UTC timestamp conversions, and resilient retry policies.
* **Row Level Security (RLS):** Fully enabled RLS on all tables with public REST permissions restricted, keeping database records 100% private.
* **Robust Authentication:** Cookie-based authentication for Web MVC and JWT Bearer tokens for API endpoints.
* **Password Hashing & Recovery:** Industry-standard BCrypt implementation with salted hashes and cryptographic, time-limited password reset tokens.
* **Auditability & Soft Deletes:** Built-in soft-delete mechanisms with EF Core Global Query Filters and timestamp tracking (`CreatedAt`, `UpdatedAt`, `DeletedAt`).

---

## 🏗️ Architecture & Technology Stack

Spendly strictly adheres to the concentric layers of **Clean Architecture**, guaranteeing decoupled business rules, high testability, and framework independence.

```
                  ┌─────────────────────────────────────────┐
                  │      Spendly.Web (ASP.NET Core MVC)     │
                  │      Spendly.Api (RESTful API & JWT)    │
                  └────────────────────┬────────────────────┘
                                       │
                  ┌────────────────────▼────────────────────┐
                  │    Spendly.Infrastructure (EF Core)     │
                  │    - PostgreSQL DbContext (Npgsql)      │
                  │    - Supabase Transaction Pooler        │
                  │    - Google Gemini AI Service           │
                  │    - MailKit SMTP Email Service         │
                  │    - Repository Implementations         │
                  └────────────────────┬────────────────────┘
                                       │
                  ┌────────────────────▼────────────────────┐
                  │    Spendly.Application (Use Cases)      │
                  │    - DTOs, Mappings & Interfaces        │
                  │    - BudgetAlertService & Engine        │
                  │    - AI Parsing & Suggestion Contracts  │
                  └────────────────────┬────────────────────┘
                                       │
                  ┌────────────────────▼────────────────────┐
                  │       Spendly.Domain (Core)             │
                  │    - Entities & Value Objects (Money)   │
                  │    - Domain Exceptions & Invariants     │
                  └─────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Project | Key Technologies & Responsibilities |
| :--- | :--- | :--- |
| **Domain** | `Spendly.Domain` | Enterprise core: `Expense`, `Budget`, `Income`, `SavingsGoal`, `RecurringExpense`, `User`, `Category`, `Tag`. Encapsulates `Money` Value Object, Domain Exceptions, and pure business invariants. Zero external dependencies. |
| **Application** | `Spendly.Application` | Business use cases, DTOs, repository abstractions (`IExpenseRepository`, `IBudgetRepository`), services (`BudgetAlertService`), and AI contracts (`IAiAssistantService`). |
| **Infrastructure** | `Spendly.Infrastructure` | Data access with **Entity Framework Core** and **Npgsql**, PostgreSQL migrations, Supabase connection pooler, Google Gemini AI integration, SMTP email delivery via **MailKit**, and BCrypt password encryption. |
| **Web Presentation** | `Spendly.Web` | Client-facing **ASP.NET Core MVC** application with Razor Views, custom Vanilla CSS design system, Chart.js, ApexCharts, bilingual localization (`SharedResource.*.resx`), responsive layouts, and cookie authentication. |
| **API Presentation** | `Spendly.Api` | RESTful API endpoints, JWT token issuance, Swagger documentation, and CORS configuration. |
| **Test Suite** | `Spendly.Tests` | Comprehensive unit and integration test suite (**124 passing tests**, 100% green) using **xUnit**, **Moq**, and **FluentAssertions**. |

---

## 🧪 Testing & Code Quality

* **124 Automated Tests:** Exhaustive coverage across Domain entities, invariants, Value Objects, Use Cases, AI DTO parsers, and the automated Budget Alert Engine.
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
* [PostgreSQL](https://www.postgresql.org/) (Local instance or free [Supabase](https://supabase.com/) project)
* Visual Studio 2022 / VS Code / JetBrains Rider

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Manushark/Spendly.git
   cd Spendly
   ```

2. **Configure Database & Connection String**
   * Update `src/Spendly.Api/appsettings.Development.json` with your PostgreSQL / Supabase connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.your_project;Password=your_password;SSL Mode=Require;Trust Server Certificate=true;"
   }
   ```

3. **Apply EF Core Migrations & Seed Data**
   ```bash
   dotnet ef database update --project src/Spendly.Infrastructure --startup-project src/Spendly.Api
   ```
   *(Or simply run `Spendly.Api`—the built-in `DatabaseMigrationService` will automatically check and apply pending migrations).*

4. **Run the Application**
   ```bash
   # Terminal 1: Run the RESTful API
   dotnet run --project src/Spendly.Api --launch-profile https

   # Terminal 2: Run the Web MVC Application
   dotnet run --project src/Spendly.Web --launch-profile https
   ```
   Open your browser at `https://localhost:7024`.

---

## 🗺️ Versioning & Changelog

Spendly follows [Semantic Versioning](https://semver.org/). Recent milestones:
* **v1.27.0 (Current):** Animated spam thought cloud in password recovery, full viewport dark background coverage, complete data migration to Supabase PostgreSQL, and sequence alignment.
* **v1.26.0:** PostgreSQL migration on Supabase, elimination of cold starts, UTC timestamp normalization, multi-model Google Gemini Copilot resilience, and Insights chart character encoding fix.
* **v1.25.0:** Dual chart mode toggle (Monthly bars vs. 30-day daily trend line) on Dashboard and Reports with full bilingual localization.
* **v1.22.2:** Mobile Compact KPI Grid 2x2 layout, compiler null-safety cleanup, automated tag synchronization.
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
