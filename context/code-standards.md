# Code standards — Bright Path Learning Centre

Referenced from `CLAUDE.md`. Naming and API-design conventions live here — follow
them for all code in this repo.

## Naming
- Spell names out in full, no abbreviations: `formatTime` not `fmt`, `response`
  not `res`, `rowClassName` not `cls`, `cancellationToken` not `ct`. Established
  domain acronyms (`Api`, `Id`, `csv`) are fine.
- No single-character names, loop indices included (`index`, `lesson`, `event`).
  In `frontend/` this is enforced by ESLint's `id-length` rule — run
  `npm run lint`.
- Keep a blank line between a closing brace and the statement that follows it
  (so an early `return` after a guard block stands apart). In `frontend/` this
  is ESLint's `@stylistic/padding-line-between-statements` (`npm run lint --
  --fix`); in the backend it is Roslynator `RCS0008`, set to `error` in
  `.editorconfig`, so `dotnet build` fails on it — `dotnet format` fixes it.

## API design
- One endpoint per responsibility. When a response starts carrying an unrelated
  concern, split it into its own endpoint/resource instead of growing the
  payload — e.g. per-tutor load is `GET /api/tutors/loads`, not a field on
  `GET /api/schedule` (`DECISIONS.md` §9). This is the read-side of "one
  endpoint per intent" below.
