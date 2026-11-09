# AGENTS.md

## Workflow

1. **No co-author trailers.** Never add `Co-authored-by`, `Generated with`, or similar trailers to commits or PR descriptions.
2. **One commit per change.** Every distinct change gets its own commit. Never bundle unrelated changes.
3. **Conventional commits.** Format: `type: short description in English`. Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `style`, `build`, `ci`. Lowercase, imperative mood, no trailing period, no scope unless needed (`fix(auth): ...`).
4. **Pull requests in English.** Descriptive title and body, describing only the changes made — no fluff, no co-author lines.
5. **Branch by size.** Large changes go on a separate branch with a PR. Small changes go directly to `main`.
6. **Planning and coding style.** Activate the `ponytail` and `caveman` skills when starting a plan or executing code.
