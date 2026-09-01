# Spendly — Local Setup Guide

## Prerequisites

| Tool | Version | Download |
|------|---------|---------|
| .NET SDK | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| SQL Server | 2019+ or LocalDB | [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |
| Git | Any | [git-scm.com](https://git-scm.com) |

---

## 1. Clone the repository

```bash
git clone https://github.com/Manushark/Spendly.git
cd Spendly
```

---

## 2. Configure the database connection

Open `src/Spendly.Api/appsettings.Development.json` and set your SQL Server connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SpendlyDb;Trusted_Connection=True;"
  }
}
```

Do the same for `src/Spendly.Web/appsettings.Development.json`.

> **Tip:** If using SQL Server Express, replace `(localdb)\\mssqllocaldb` with `.\SQLEXPRESS`.

---

## 3. Configure JWT

In `src/Spendly.Api/appsettings.Development.json`, add:

```json
{
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-at-least-32-chars",
    "Issuer": "Spendly",
    "Audience": "SpendlyUsers"
  }
}
```

> ⚠️ Use a random 32+ character string. Never commit real secrets to Git.

---

## 4. Run migrations

```bash
cd src/Spendly.Infrastructure
dotnet ef database update --startup-project ../Spendly.Api
```

This creates the database schema automatically.

---

## 5. Run the API

```bash
cd src/Spendly.Api
dotnet run
```

The API starts on `https://localhost:7xxx` (check the console output).

---

## 6. Run the Web App

Open a second terminal:

```bash
cd src/Spendly.Web
dotnet run
```

The MVC app starts on a different port and calls the API.

> **Important:** Make sure the `ApiBaseUrl` in `Spendly.Web/appsettings.Development.json` points to the correct API port.

---

## 7. Run the tests

```bash
dotnet test Spendly.Tests/Spendly.Tests.csproj --verbosity minimal
```

Expected output: **57 tests passing, 0 failures.**

---

## Common Issues

### ❌ `Invalid object name 'Expenses'`
The database doesn't exist yet. Run Step 4 (migrations).

### ❌ `Connection string not found`
Check that you edited the correct `appsettings.Development.json`, not `appsettings.json`.

### ❌ JWT validation fails
Make sure the same `Jwt:Key` is configured in both the API and checked in the Web app's `appsettings`.

### ❌ `dotnet ef` not found
Install the EF tools:
```bash
dotnet tool install --global dotnet-ef
```
