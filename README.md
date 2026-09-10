# Tutoring Scheduling

Internal scheduling tool for **Bright Path Learning Centre** (Da Nang).

The one feature built here is **rescheduling a lesson**: a single intent
endpoint that checks the target slot against the conflict engine, records the
change in an append-only log, and makes a change past the daily cut-off
**visible as a change** rather than a silent overwrite. A read-only React room
board (`frontend/`) shows the result.

Stack: ASP.NET Core Minimal API, EF Core, SQL Server — laid out as a four-project
Clean Architecture solution (see [Layout](#layout)).

## Run it

The .NET side lives under `backend/` (solution, projects, `seed-data/`); run the
commands below from there.

Prerequisite: a local SQL Server instance reachable at `localhost` with Windows
authentication (the connection string lives in `backend/Api/appsettings.json`
under `ConnectionStrings:Default`).

```bash
cd backend
dotnet run --project Api
```

On start the app applies migrations, creates the `TutoringScheduling` database,
and seeds it from the CSV export under `backend/seed-data/` (34 lessons, week of
2026-03-03 … 03-10), loaded verbatim including the historical conflicts. It also
writes a `Created` event per lesson and reconstructs `L032`'s "moved from Sunday"
history, which the export itself lost. Drop the `TutoringScheduling` database to
re-seed.

This is a JSON API — no page at `/`. Call the endpoints directly, or run the
board (below).

### The board (`frontend/`)

The room board is a React 18 + TypeScript (Vite) app under `frontend/`. Run the
API as above, then:

```bash
cd frontend
npm install
npm run dev
```

and open http://localhost:5173 (Vite proxies `/api` to the API). See
`frontend/README.md`.

```bash
cd backend
dotnet test
```

runs the conflict-engine and service tests (unit + SQL Server-backed) plus the
architecture test that enforces the layer boundaries. The SQL Server-backed
tests each create and drop a throwaway `TutoringScheduling_Test_*` database on
`localhost`, so `dotnet test` also needs that instance reachable.

### "Now"

The brief pins the clock to a value inside the seeded week, never the real
system clock. It is **2026-03-06 09:00 (+07:00)**, set by `Schedule:Now` in
`appsettings.json`, and drives the "after 16:00 the day before" cut-off in
`MoveLessonService`. The read endpoints take an explicit `date`; the board's
default day is a client-side constant (`PINNED_TODAY` in
`frontend/src/api/schedule.ts`), set to the same date.

## The feature — reschedule a lesson

### `POST /api/lessons/{id}/move`

```json
{
  "toDate": "2026-03-07",
  "toStartTime": "14:00:00",
  "toRoomId": "R4",
  "reason": "family asked to move it later"
}
```

`toRoomId` is optional (keeps the current room). The move is **rejected**
(`409`) when it would create an error-level conflict (tutor / room / student
double-booked), when the lesson is cancelled, part of an exam pair, or the
target day is a Monday. Non-blocking issues (e.g. the tutor going over 6
lessons that day) come back as `warnings`, not a rejection.

On success the lesson row is updated and a `Moved` event is appended with
`afterCutoff` set when the change was made after 16:00 the day before the
lesson.

### `GET /api/lessons/{id}/history`

The lesson's current state plus its event log, oldest first.

## Supporting read views

The board (`frontend/`) fetches three endpoints:

| Endpoint | Purpose |
| --- | --- |
| `GET /api/schedule?date=` | One day's board: rooms, per-lesson conflict codes, `movedAfterCutoff`, post-cut-off changes |
| `GET /api/tutors/loads?date=` | Per-tutor lesson count for the day and the over-the-6-limit flag |
| `GET /api/conflicts?from=&to=` | The day's conflict list for the banner (`from` = `to` = the board date) |

`date` is **required** on the first two (`400` without it).

### `GET /api/schedule?date=YYYY-MM-DD`

One day, grouped by room (all six, empty ones included). Each lesson carries
its conflict codes and a `movedAfterCutoff` flag; the response also has a
`changes` list of post-cut-off moves touching that day. The day's conflict
list is served separately by `GET /api/conflicts` (below). `date` is
**required** — a request without it returns `400`.

### `GET /api/tutors/loads?date=YYYY-MM-DD`

Per-tutor lesson count for the day and an `overLimit` flag for anyone over the
soft 6-lessons-a-day limit. `date` is **required** (`400` without it).

### `GET /api/conflicts?from=&to=`

Runs the conflict engine over stored bookings (optionally within a range) and
returns every clash plus an error/warning count. The board calls this with
`from` = `to` = the current day to fill its banner.

## Conflict codes

| Code | Severity | Meaning |
| --- | --- | --- |
| `TUTOR_DOUBLE_BOOKED` | error | Same tutor in two overlapping lessons |
| `ROOM_DOUBLE_BOOKED` | error | Same room, two overlapping lessons, not an exam pair |
| `STUDENT_DOUBLE_BOOKED` | error | Same student in two overlapping lessons |
| `TUTOR_DAILY_LIMIT` | warning | A tutor is over the 6-lessons-a-day limit |
| `CENTRE_CLOSED_MONDAY` | warning | A lesson falls on a Monday |

Cancelled bookings are ignored (the slot is free); no-shows still occupy it.
Lessons sharing a `GroupId` are a sanctioned exam pair and never clash with
each other. The seed export has two errors (`L033`/`L034`, `L007`/`L008`) and
two warnings (tutor `T1` over limit on 2026-03-06, Monday lesson `L032`).

## Layout

The .NET solution sits under `backend/`; `frontend/` and the docs stay at the
repo root. Four projects, dependencies pointing inward only
(`Api → Infrastructure → Application → Domain`):

```
backend/
  TutoringScheduling.slnx, Directory.Build.props
  seed-data/       lessons_export.csv, tutors.csv
  Domain/          entities, the centre calendar (UTC+7, 16:00 cut-off). No dependencies.
  Application/     use cases + API contracts. References Domain only.
                   UseCases/    MoveLessonService, ScheduleService, TutorService
                                (each behind an I…Service interface; DI maps it)
                   Abstractions/ IClock + the persistence ports
                                (IBookingStore, IRoomStore, ILessonEventStore, IMoveRecorder)
                   Contracts/   request/response DTOs
                   ConflictDetector (pure), LessonMapper
  Infrastructure/  EF Core implementation of the ports. References Application.
                   Persistence/ AppDbContext, migrations, the EF Core stores, CSV seeder
                   Time/        PinnedClock
  Api/             composition root: Minimal API endpoints, startup. JSON only.
                   Endpoints/   one file per route
  Tests/           xUnit; SqlServerFixture + FixedClock back the service tests;
                   ArchitectureTests enforces the dependency rule
frontend/          React 18 + TS (Vite) room board; dev-proxies /api
```

The use-case layer never sees a `DbContext` — it goes through the persistence
ports (`IBookingStore`, `IRoomStore`, `ILessonEventStore`, `IMoveRecorder`),
defined in `Application` and implemented in `Infrastructure`. `AddApplication()`
and `AddInfrastructure(config)` wire each layer up in `Api/Program.cs`.

EF Core migrations (from `backend/`, startup project is `Api`):

```bash
dotnet ef migrations add <Name> --project Infrastructure --startup-project Api
```
