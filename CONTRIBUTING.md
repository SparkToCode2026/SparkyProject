# Contributing / Git Workflow

- One feature branch per developer, per sprint task, e.g.:
  - `feature/dev1-user-hotel-models`
  - `feature/dev3-booking-controller`
  - `feature/dev5-review-frontend`
- Never commit directly to `main`. Open a Pull Request and get at least one teammate's review
  before merging.
- Keep PRs scoped to your 2 owned models where possible, so reviews stay small and git history
  clearly shows each developer's contribution (this is an evaluation criterion).
- Rebase/pull `main` before starting new work each day to avoid large conflicts, especially in
  shared files like `AppDbContext.cs` and `Program.cs`.
- Use GitHub Issues to track each of your 8 required cases per controller as checkable tasks.

## Commit message style

Keep it simple and descriptive, e.g.:

```
feat(booking): add GET filter by date range
fix(user): correct password hash comparison
docs(erd): add Booking-Payment relationship
```
