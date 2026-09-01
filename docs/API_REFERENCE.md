# Spendly — API Reference

> Base URL: `https://spendly-web-cncja8b2edephcd6.westus2-01.azurewebsites.net/api`  
> All protected endpoints require a `Bearer` token in the `Authorization` header.

---

## Authentication

### POST `/api/auth/login`
Login with email and password.

**Rate Limited** · No auth required

**Body:**
```json
{ "email": "user@example.com", "password": "yourpassword" }
```

**Response `200`:**
```json
{ "token": "eyJhbGci..." }
```

**Errors:** `401` Invalid credentials · `400` Invalid email format

---

### POST `/api/auth/register`
Create a new account. Automatically seeds default categories.

**Rate Limited** · No auth required

**Body:**
```json
{
  "email": "user@example.com",
  "password": "min6chars",
  "confirmPassword": "min6chars"
}
```

**Response `200`:**
```json
{ "token": "eyJhbGci..." }
```

**Errors:** `400` Email taken · `400` Passwords don't match · `400` Password too short

---

## Expenses

All endpoints require `Authorization: Bearer <token>`

### GET `/api/expenses`
List expenses with filtering and pagination.

**Query Params:**
| Param | Type | Description |
|-------|------|-------------|
| `category` | string | Filter by category name |
| `search` | string | Full-text search on description |
| `dateFrom` | datetime | Start date |
| `dateTo` | datetime | End date |
| `minAmount` | decimal | Minimum amount |
| `maxAmount` | decimal | Maximum amount |
| `tagIds` | int[] | Filter by tag IDs |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (max: 100, default: 10) |

**Response `200`:** Paginated list of expenses.

---

### POST `/api/expenses`
Create a new expense. **Rate Limited**

**Body:**
```json
{
  "amount": 25.50,
  "category": "Food & Dining",
  "description": "Lunch at café",
  "date": "2026-07-12",
  "tagIds": [1, 3]
}
```

**Response `201`:** `{ "id": 42 }`

---

### GET `/api/expenses/{id}`
Get a single expense by ID.

**Response `200`:** Expense object · **`404`** Not found

---

### PUT `/api/expenses/{id}`
Update an expense. **Rate Limited**

**Response `204`** No content · **`403`** Not your expense

---

### DELETE `/api/expenses/{id}`
Soft-delete an expense. **Rate Limited**

**Response `204`** · **`404`** Not found

---

### GET `/api/expenses/export/csv`
Export expenses to CSV.

**Query Params:** `category`, `dateFrom`, `dateTo`  
**Response:** `text/csv` file download

---

### GET `/api/expenses/export/report`
Export monthly report as HTML.

**Query Params:** `month` (1–12), `year`  
**Response:** `text/html`

---

## Budgets

### GET `/api/budgets` — List all budgets with spending data
### POST `/api/budgets` — Create budget `{ category, monthlyLimit, year, month }`
### GET `/api/budgets/{id}` — Get budget with real-time spending %
### PUT `/api/budgets/{id}` — Update budget
### DELETE `/api/budgets/{id}` — Delete budget

---

## Incomes

### GET `/api/incomes` — List incomes (paginated)
### POST `/api/incomes` — Create income `{ amount, currency, source, description, date, isRecurring }`
### GET `/api/incomes/{id}` — Get income
### PUT `/api/incomes/{id}` — Update income
### DELETE `/api/incomes/{id}` — Delete income

---

## Categories

### GET `/api/categories` — List user's categories (max 50)
### POST `/api/categories` — Create category `{ name, icon, color }`
### PUT `/api/categories/{id}` — Update category
### DELETE `/api/categories/{id}` — Delete (fails if expenses/budgets use it)

---

## Savings Goals

### GET `/api/savingsgoals` — List all goals with progress %
### POST `/api/savingsgoals` — Create goal `{ name, targetAmount, currentAmount, deadline, icon, color }`
### GET `/api/savingsgoals/{id}` — Get goal
### PUT `/api/savingsgoals/{id}` — Update goal
### DELETE `/api/savingsgoals/{id}` — Delete goal
### POST `/api/savingsgoals/{id}/funds` — Add funds `{ amount }` — triggers milestone notifications

---

## Tags

### GET `/api/tags` — List tags with usage count
### POST `/api/tags` — Create tag `{ name, color }`
### PUT `/api/tags/{id}` — Update tag
### DELETE `/api/tags/{id}` — Delete tag

---

## Notifications

### GET `/api/notifications` — List unread notifications
### POST `/api/notifications/{id}/read` — Mark as read
### POST `/api/notifications/read-all` — Mark all as read
### DELETE `/api/notifications/{id}` — Delete notification

---

## Recurring Expenses

### GET `/api/recurringexpenses` — List recurring rules
### POST `/api/recurringexpenses` — Create rule `{ category, amount, description, frequency, startDate }`
### PUT `/api/recurringexpenses/{id}` — Update rule
### DELETE `/api/recurringexpenses/{id}` — Delete rule
### POST `/api/recurringexpenses/{id}/toggle` — Enable/disable rule

---

## Dashboard

### GET `/api/dashboard` — Summary: total income, total expenses, balance, top categories

---

## Insights

### GET `/api/insights` — Financial insights engine output for current user

---

## Reports

### GET `/api/reports/monthly` — Detailed monthly report data
### GET `/api/reports/annual` — Annual summary

---

## Import

### POST `/api/import/csv` — Upload CSV file to import expenses (multipart/form-data)

---

## Error Format

All errors return:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "A category named 'Food' already exists."
}
```
