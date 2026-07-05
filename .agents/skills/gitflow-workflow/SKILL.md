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

### 2. Implement the feature with clean commits
```bash
git add .
git commit -m "feat: <what you did>"
```

Commit message conventions:
- `feat:` -- new feature
- `fix:` -- bug fix
- `docs:` -- documentation only
- `test:` -- adding or updating tests
- `refactor:` -- code change without new feature or fix
- `style:` -- formatting, no logic change

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
4. When completing a feature, remind the user to merge back to `develop`.
5. Update `CHANGELOG.md` when the feature is merged.
