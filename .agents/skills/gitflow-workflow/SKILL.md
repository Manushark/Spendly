---
name: gitflow-workflow
description: >
  Enforces GitFlow branching strategy for the Spendly project. Must be applied
  before implementing any new feature, fix, or improvement. Ensures code never
  goes directly to main or develop without proper branch management.
---

# GitFlow Workflow — Spendly

## MANDATORY: Read this BEFORE making any code change

Every new feature, fix, or improvement in Spendly **MUST** follow GitFlow.
**Never commit directly to `main` or `develop`.**

---

## Branch Naming Convention

| Type | Pattern | When to use |
|------|---------|-------------|
| New feature | `feature/<short-description>` | New functionality |
| Bug fix | `fix/<short-description>` | Non-urgent bug |
| Urgent hotfix | `hotfix/<short-description>` | Critical production bug |
| Release prep | `release/<version>` | Release stabilization |

**Examples:**
- `feature/unit-tests-budget-savings`
- `feature/forgot-password`
- `fix/notification-badge-count`
- `hotfix/jwt-expiry-crash`

---

## Step-by-Step Workflow

### 1. Create the feature branch from `develop`
```bash
git checkout develop
git pull origin develop
git checkout -b feature/<description>
```

### 2. Implement the feature with GRANULAR commits

**CRITICAL RULE: Maximum 1-2 files per commit.**
Each commit must represent a single, traceable reason for change.
This makes `git log` and `git blame` actually useful.

```bash
# CORRECT: commit one file at a time
git add src/Spendly.Domain/Entities/MyEntity.cs
git commit -m "feat(domain): add MyEntity with validation"

git add src/Spendly.Application/UseCase/MyFeature/MyUseCase.cs
git commit -m "feat(application): add MyUseCase for MyEntity"

git add Spendly.Tests/UseCases/MyFeature/MyUseCaseTests.cs
git commit -m "test(myfeature): add unit tests for MyUseCase"

# WRONG: never bundle unrelated files together
# git add .
# git commit -m "add everything"
```

Commit message conventions (Conventional Commits):
- `feat(scope):` -- new feature
- `fix(scope):` -- bug fix
- `docs(scope):` -- documentation only
- `test(scope):` -- adding or updating tests
- `refactor(scope):` -- code change without new feature or fix
- `style(scope):` -- formatting, no logic change
- `chore(scope):` -- tooling, config, skills, non-src changes

Scope examples: `domain`, `application`, `infrastructure`, `api`, `web`, `budgets`, `expenses`, `savings`

**Good commit message examples:**
- `feat(domain): add RecurringExpense.Pause() method`
- `test(budgets): add DeleteBudgetUseCase ownership validation test`
- `fix(notifications): prevent duplicate budget alert for same threshold`
- `chore: update gitflow skill with granular commit rule`

### 3. When done, merge back to `develop`
```bash
git checkout develop
git merge feature/<description> --no-ff
git push origin develop
```

### 4. Delete the feature branch
```bash
git branch -d feature/<description>
```

---

## Active Branches in Spendly

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready code (deployed to Azure) |
| `develop` | Integration branch -- always latest stable |
| `feature/*` | Work in progress features |
| `fix/*` | Bug fixes |
| `hotfix/*` | Emergency production fixes |

---

## UX Feature Branches (Pending)

These are the planned UX branches from the design system roadmap:
- `feature/ux-theme-base` -- CSS variables, layouts
- `feature/ux-dashboard` -- Dashboard improvements
- `feature/ux-expenses` -- Expenses filters, modals
- `feature/ux-recurring` -- Recurring expenses cards
- `feature/ux-reports` -- Reports, stacked charts
- `feature/ux-insights` -- Insights, projections
- `feature/ux-settings` -- Settings, category tabs

---

## Rules for the Agent

1. **ALWAYS** create a `feature/` branch before making code changes.
2. **ALWAYS** tell the user which branch was created and why.
3. **NEVER** commit directly to `main` or `develop`.
4. **ALWAYS** commit 1-2 files maximum per commit — NEVER use `git add .` to stage everything at once.
5. **ALWAYS** use a descriptive commit message with scope: `type(scope): description`.
6. When completing a feature, remind the user to merge back to `develop`.
7. Update `CHANGELOG.md` when the feature is merged.

### Commit Granularity Guide

If you're adding a new feature with multiple layers, the commit sequence should look like:

```
feat(domain):         add the entity/value object
feat(infrastructure): add the EF configuration and migration
feat(application):    add the interface and use case
feat(api):            add the controller endpoint
feat(web):            add the view and API client
test(module):         add unit tests for the use case
docs:                 update CHANGELOG.md
```

Each commit = one layer = one reason to exist in git history.
