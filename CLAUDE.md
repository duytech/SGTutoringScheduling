# Bright Path Learning Centre — take-home exercise

## What this is
Internal scheduling tool for a tutoring centre (Da Nang). Full brief and
grading rubric: see `brief-notes.md`. Seed data:
`backend/seed-data/lessons_export.csv` (35 rows, week of 2026-03-03 to
2026-03-10) and `backend/seed-data/tutors.csv`.

Time-boxed to ~2.5 hours. Build exactly one feature, fully correct, over
several partial ones.

## Stack
Any stack I'm fastest in — see `brief-notes.md` for the candidate's default
(ASP.NET Minimal API + EF Core + SQLite) unless told otherwise in chat.

The .NET solution lives under `backend/` (`frontend/` and the docs stay at the
repo root). It is a four-project Clean Architecture solution
(`Api → Infrastructure → Application → Domain`; dependencies point inward only).
The use-case services reach persistence through the per-aggregate ports in
`Application/Abstractions` (`IBookingStore`, `IRoomStore`, `ILessonEventStore`,
`IMoveRecorder`), implemented in `Infrastructure` — they never see a `DbContext`.
From `backend/`: run with `dotnet run --project Api`; `dotnet ef` uses
`--project Infrastructure --startup-project Api`. Layer map and rationale:
`DECISIONS.md` §15.

## Pinned "today"
Use 2026-03-06 (or another date inside 2026-03-03..2026-03-10) as "today" —
never the real system clock. State which date is used in the README.

## Code standards
Naming and API-design conventions live in `context/code-standards.md` — follow
them for all code in this repo.

## Rules from the brief (see brief-notes.md for full context/contradictions)
- 6 rooms, one lesson per room at a time, a tutor in one room at a time.
- No tutor gets more than 6 bookings/day — but the business itself already
  breaks this in the seed data. Treat as a soft/warn rule in code, not a hard
  DB constraint, unless told otherwise.
- Family may cancel free >4h before start; inside 4h, full charge + tutor
  still paid. Cancelling frees the room/slot; a no-show frees neither.
- Exam pairs: same tutor+room+slot, two students, half price — a deliberate,
  sanctioned exception, not a bug. Must be created via an explicit
  "pairing" action, never inferred from a coincidental duplicate.
- Changes made after 16:00 the day before must be visible as changes, not
  silent overwrites of what a tutor was already told.

## Don't
- Don't build a generic `PUT /lessons/{id}`. Use one endpoint per intent
  (create / move / cancel / mark-no-show / create-pair).
- Don't treat "same tutor+room+slot" as always invalid — check `group_id`
  (exam pair) first.
- Don't build UI polish, auth, or CRUD-everything unless asked — see
  "What I leave broken" conventions in `brief-notes.md`.
- Don't reference `Microsoft.EntityFrameworkCore` or `Microsoft.AspNetCore`
  from the `Domain` or `Application` projects — `Tests/ArchitectureTests` fails
  the build if you do. Don't collapse the persistence ports back into a direct
  `AppDbContext` dependency; they're a deliberate choice (`DECISIONS.md` §15).
- Don't commit or push directly to `main`. Always cut a feature branch,
  push it, and open a PR — even for small changes. Sync `main` only by
  pulling after the PR merges.

## Deliverables expected
- `DECISIONS.md` at repo root (four sections: questions for the owner,
  feature chosen + why, data model/API for it, reflection).
- Working code, seeded from `backend/seed-data/`, with run commands in README.
- Atomic commits, one change each, not squashed.
