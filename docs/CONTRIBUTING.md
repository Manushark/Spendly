# Contributing to Spendly

Thank you for your interest in contributing! This guide explains the workflow and standards used in this project.

---

## Branching Strategy (GitFlow)

We follow **GitFlow**. All work happens in feature branches — never directly on `main` or `develop`.

```
main          ← production-ready releases only
  └── develop ← integration branch
        ├── feature/your-feature-name    ← new features
        ├── fix/short-description        ← bug fixes
        └── hotfix/critical-fix          ← urgent production fixes
```

### Creating a branch

Always branch **from `develop`**:

```bash
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name
```

### Merging back

When the feature is done and tested:

```bash
git checkout develop
git merge --no-ff feature/your-feature-name
```

---

## Commit Convention

We use **Conventional Commits** with scopes:

```
<type>(<scope>): <short description>
```

**Types:**
| Type | When to use |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `test` | Adding or modifying tests |
| `refactor` | Code change without new feature or fix |
| `chore` | Config, tooling, scripts |
| `docs` | Documentation only |

**Scopes** (match the domain):
`auth` · `expenses` · `budgets` · `incomes` · `savings` · `categories` · `tags` · `notifications` · `reports` · `services` · `agents`

**Examples:**
```
feat(budgets): add monthly limit alert at 80% threshold
fix(auth): normalize email before lookup to prevent case mismatch
test(categories): add CreateCategoryUseCase tests (50-limit, duplicate name)
docs(api): add API_REFERENCE.md with all 14 controllers
```

---

## Granular Commits — Important Rule

> **One commit = one file or one very small logical change.**

This makes `git log` readable and `git bisect` useful. Avoid bundling unrelated changes into a single commit.

```bash
# ✅ Good
git add Spendly.Tests/UseCases/Auth/LoginUseCaseTests.cs
git commit -m "test(auth): add LoginUseCase tests"

git add Spendly.Tests/UseCases/Auth/RegisterUseCaseTests.cs
git commit -m "test(auth): add RegisterUseCase tests"

# ❌ Avoid
git add .
git commit -m "add tests"
```

---

## Adding a New Feature

1. **Read the Use Case layer first.** All business logic lives in `Spendly.Application/UseCase/`.
2. **Add the Use Case.** One class, one `ExecuteAsync` method.
3. **Add the API endpoint** in `Spendly.Api/Controllers/`.
4. **Add the Web client** in `Spendly.Web/Services/`.
5. **Add unit tests** in `Spendly.Tests/UseCases/`.

### Minimum tests per Use Case
- ✅ Happy path (success)
- ✅ Not found (returns null or throws)
- ✅ Unauthorized (wrong userId)
- ✅ Invalid input (domain validation)

---

## Running Tests Locally

```bash
# Run all tests
dotnet test Spendly.Tests/Spendly.Tests.csproj --verbosity minimal

# Run only tests for a specific module
dotnet test --filter "FullyQualifiedName~Categories"
```

All tests must pass before merging.

---

## Code Style

- **One class per file** — always.
- **Private setters on entity properties** — domain entities control their own state.
- **Validation in the Domain** — not in controllers or DTOs.
- **Use `async/await`** — no `.Result` or `.Wait()`.
- **No magic strings** — use constants or enums for repeated values.
