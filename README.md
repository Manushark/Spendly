<div align="center">
  <!-- Puedes agregar un logo aquí más adelante -->
  <h1>💸 Spendly</h1>
  <p><strong>A Modern & Secure Personal Finance Management Platform</strong></p>

  <p>
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet" alt=".NET" />
    <img src="https://img.shields.io/badge/Architecture-Clean_Architecture-2ea44f?style=flat-square" alt="Clean Architecture" />
    <img src="https://img.shields.io/badge/SQL_Server-Database-red?style=flat-square&logo=microsoftsqlserver" alt="SQL Server" />
    <img src="https://img.shields.io/badge/License-Non--Commercial-blue?style=flat-square" alt="License" />
    <img src="https://img.shields.io/badge/Status-Active_Development-success?style=flat-square" alt="Status" />
  </p>
</div>
   <p>
    🌐 <strong>Live Demo:</strong><br/>
    <a href="https://spendly-web-cncja8b2edephcd6.westus2-01.azurewebsites.net/" target="_blank">
      Spendly Web App
    </a>
  </p>
<br/>

## 📖 Overview 

Spendly is a full-stack financial management application designed to help users manage expenses, budgets, and recurring transactions in a clean and modern experience.

The project is currently under active development and is being built with scalability, maintainability, and production-ready practices in mind.

## ✨ Features

### Current Features
* Expense tracking and management
* Budget creation and monitoring
* Recurring expense support
* JWT-based authentication & Google OAuth
* Responsive UI and modern dashboard layouts
* Pagination and filtering support
* Improved validation and error handling
* Financial reports and analytics
* Notifications and reminders

### Planned Features
* Mobile application support (.NET MAUI / Flutter)
* Premium subscription system
* AI-powered spending insights

## 🏗️ Architecture & Tech Stack

The project strictly adheres to **Clean Architecture** principles, separating core business logic from infrastructure and presentation concerns.

* **Backend:** ASP.NET Core 8, C#, JWT Authentication
* **Frontend:** ASP.NET Core MVC, Razor Views, Vanilla CSS
* **Data Access:** Entity Framework Core
* **Database:** SQL Server

### Project Structure
```bash
src/
├── Spendly.Api           # Entry point, Controllers, Dependency Injection
├── Spendly.Application   # Use Cases, DTOs, Interfaces
├── Spendly.Domain        # Core Entities, Enums, Exceptions
├── Spendly.Infrastructure# EF Core DB Context, Repositories
└── Spendly.Web           # Client-facing MVC Application
```

## 🔒 Security & Configuration

This project follows environment-based configuration practices:
* Sensitive configuration is separated from base configuration files.
* Production secrets are intended to be stored using environment variables or user secrets.
* JWT authentication is configurable and environment-aware.

Example environment variables can be found in `.env.example`.

## 🚀 Getting Started

To get a local copy up and running, follow these steps.

### Prerequisites
* .NET 8.0 SDK
* SQL Server
* Visual Studio 2022 or VS Code

### Installation & Run

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/Spendly.git
   cd Spendly
   ```

2. **Configure the Database**
   * Update `appsettings.Development.json` in the API project with your local database connection string.

3. **Apply Migrations**
   ```bash
   cd src/Spendly.Infrastructure
   dotnet ef database update --startup-project ../Spendly.Api
   ```

4. **Run the Applications**
   Since this is a decoupled architecture, you must run both the API and the Web project simultaneously:
   
   **Terminal 1 (API):**
   ```bash
   cd src/Spendly.Api
   dotnet run
   ```
   
   **Terminal 2 (Web Client):**
   ```bash
   cd src/Spendly.Web
   dotnet run
   ```

## 🗺️ Roadmap

* [ ] Implement Mobile Application
* [ ] Integrate Subscription-based premium features (Stripe/RevenueCat)
* [x] Cloud deployment & CI/CD pipeline
* [x] Advanced reporting system

## ⚖️ License

**Copyright (c) 2026 Manuel Rivas. All rights reserved.**

This project is licensed under a custom **Non-Commercial License**. You may use and study this code for personal and educational purposes only. Commercial use, redistribution, or monetization of this project without explicit permission is strictly prohibited. 

See the `LICENSE` file for full details.

---

## 👨‍💻 Author

**Manuel Rivas T.**  
*Software developer passionate about backend development, software architecture, and scalable application design.*
