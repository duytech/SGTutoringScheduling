# DECISIONS

## 1. Questions I would ask the owner first

- **Exam pairs** — an accepted product rule, or something Mai does by hand?
  I assume accepted, and model it as lessons that share a `group_id`, created
  by an explicit pairing action. The engine treats a pair as one occupancy
  block; the move endpoint refuses to move a paired lesson on its own.
  If it is *not* accepted, those rows are historical workarounds and I would
  flag them like any other duplicate.

- **The historical export** — preserve exactly even where it breaks the
  rules, or reject bad rows on import? I assume preserve. The database loads
  the CSV as-is and the engine reports the breaches on top.

- **Cancelled vs no-show** — does the slot free up? I assume **cancelled
  frees** it, **no-show does not** (matches the fee rules). The engine ignores
  cancelled bookings and keeps no-shows.

- **The 16:00 cut-off** — "the day before" means the day before the *lesson*,
  and the centre runs on Da Nang time (UTC+7). I assumed both. Also: if a
  lesson is moved *twice*, is the cut-off judged against the original date or
  the current one? I use the date the lesson was on when the move was made
  (that is what the tutor was last told).

- **Tutor-fault cancellation** (`L017`, tutor sick, inside 4h): does the
  family fee apply? Rule as written only covers a family cancelling. Left as a
  manual-review question — out of this feature's scope.

- Does an exam pair count as **one or two** lessons in a tutor's daily load?
  I count two.

## 2. Where the brief does not fully hold together

- Owner: double-booking means "the system is broken." Receptionist:
  deliberately double-books for exam pairs. Read as: exam pairs are a
  **separate, explicit concept** (`group_id`); everything else that collides
  is a real conflict.

- "No tutor more than 6 a day" is a stated rule the business itself breaks
  (the seed has a real 7-lesson day). Read as a **warning**, not a hard block.
  A move that would push a tutor over 6 is allowed but reported as a warning.

## 3. Assumptions I had to invent

- The export is historical truth; it loads without being "fixed".
- A room hosts one lesson at a time unless the lessons are a confirmed pair.
- Only non-cancelled lessons count toward tutor load (booked + no-show).
- The seed CSV has no group column, so the exam-pair `group_id` is assigned
  at import from a known lesson-id set (`L009`, `L010`), not inferred from the
  note text.
- Lessons keep their duration and tutor across a move; only date, start time
  and room can change.

## 4. Features I considered

- Validate a new booking at creation time.
- Read-only "today" dashboard grouped by room.
- Cancellation handling with fee computation.
- **Move / reschedule with post-cut-off change tracking.**
- Tutor-facing mobile schedule view.
- Bulk import/export tooling.

## 5. The one feature I chose, and why

**Reschedule a lesson** (`POST /api/lessons/{id}/move`), with an append-only
event log and post-cut-off change visibility. The board is there to prove it
works.

Reasons:

- It is the only candidate that both **changes state** and touches the rule
  the brief states most carefully — "changes made after 16:00 the day before
  must be visible as changes, not silent overwrites." A validate-only or
  read-only feature never engages with it.
- It exercises the conflict rules for real: the move is refused when it would
  create a clash, using the **same engine** the board uses to display them —
  one implementation, not two.
- `L032`'s note ("moved from Sunday") shows the centre already does this and
  the current export loses the history. The event log is the fix.

An earlier version of this repo had a `POST /bookings/validate` endpoint that
checked a booking and persisted nothing. That was a partial feature sitting
next to this one, and its overlap check ignored exam pairs. It has been
removed.

## 6. Data model

EF Core over SQLite.

- `Tutors` — `Id`, `Name`, `Subject`, `Phone`
- `Rooms` — `Id`, `Name` (R1…R6; a table so the board shows empty rooms and
  the move endpoint can validate the target room)
- `Bookings` (a booked lesson) — `Id`, `StudentName`, `LessonDate`,
  `StartTime`, `DurationMinutes`, `TutorId` → `Tutors`, `RoomId` → `Rooms`,
  `Status` (`Booked` / `Cancelled` / `NoShow`), `GroupId` (nullable, exam
  pairs), `CancelledAt`, `Note`
- `LessonEvents` — append-only. `Id`, `LessonId` → `Bookings`, `Type`
  (`Created` / `Moved`), `OccurredAt`, the `From*` / `To*` slot fields for a
  move, `Reason`, `AfterCutoff`

The current-state row plus the event log: a move updates the row **and**
writes an event, so what a tutor was last told is never lost.

Not built: a `Student` entity (student identity is a name string here — a real
gap, see §11), a `lesson_groups` table (a bare `group_id` is enough until
pairing has its own attributes), cancel/no-show intents.

## 7. Representing a change made after the tutor was told

`LessonEvent.AfterCutoff` is set when `now > 16:00 the day before the lesson`
(`CentreCalendar.ChangeCutoff`). `GET /api/schedule` returns those events for
the day in a `changes` list and marks the lesson `movedAfterCutoff`; the board
shows them in a "tell the tutors" panel. Nothing about the change is silent.

## 8. Which rules live in the database and which in code

**Database (schema / migration):** foreign keys `Booking.TutorId`,
`Booking.RoomId`, `LessonEvent.LessonId`; required columns; non-unique indexes
on `(TutorId, LessonDate)` and `(RoomId, LessonDate)`.

**Code (the engine + the move service):** tutor / room / student overlap,
exam-pair exclusion, tutor daily limit, Monday closure, the cut-off
calculation.

**Why:** even the "physical" rules cannot be constraints here — the seed
export already violates them, so a unique index would fail the import.
Overlap is interval math, not a uniqueness check. The daily limit is a policy
the business overrides. The database guarantees referential integrity; the
engine judges everything that needs interpretation, on data the database is
required to accept.

## 9. API shape

- `POST /api/lessons/{id}/move` — the intent. One endpoint, one action; no
  generic `PUT /lessons/{id}`.
- `GET /api/lessons/{id}/history` — the audit trail.
- `GET /api/schedule?date=` — the board's data (rooms, conflicts, tutor load,
  post-cut-off changes).
- `GET /api/conflicts?from=&to=` — the engine over the whole week.

## 10. One endpoint I rejected

`PUT /lessons/{id}`. The brief calls it out, and it would make a move, a
cancellation and a room change indistinguishable in the data. Cancel and
no-show, when built, get their own intent endpoints writing their own events.

## 11. What I know is weak

- **Student identity is a name string.** Student-overlap matching is
  `OrdinalIgnoreCase` on that string — two real students with the same name
  collide. A `Student` table is the right fix.
- Overlap detection is O(n²) per day. Fine for one centre's week; for years of
  history it needs an indexed range query or a sweep line.
- Exam-pair grouping is seeded from a hard-coded id set. Real pairing needs
  its own endpoint.
- No end-to-end HTTP tests — the services and the engine are covered; the
  route wiring is only exercised by hand.
- Moving a lesson twice records both moves but the board's `changes` list does
  not collapse them into a net "here is where it is now".

## 12. Reflection — with another week

- a `Student` entity and real student references
- cancel / no-show intent endpoints with the 4-hour fee rule and the
  tutor-fault case
- move an exam pair as a unit
- HTTP-level tests with `WebApplicationFactory`
- let the board filter to one tutor (the tutor-facing view)

## 13. Where my AI assistant helped

- summarising the brief and its contradictions
- cross-checking the seed data for every conflict class
- scaffolding the EF layer, migrations and the test fixtures
- drafting this document

## 14. One suggestion I threw away

Enforcing every rule as a hard database constraint. Less code, but the seed
export — which the brief says is what really happened — would not load. The
feature exists to report on and safely change data that breaks the rules, so
the rules cannot live where they would reject that data.

## 15. Layering (Clean Architecture)

The code is split into four projects, each depending only on the ones inside
it:

| Layer | Project | Depends on | Holds |
| --- | --- | --- | --- |
| Entities | `Domain` | — | `Booking`, `LessonEvent`, `Room`, `Tutor`, `CentreCalendar`, the status/limit rules |
| Use cases | `Application` | Domain | `MoveLessonService`, `ScheduleService`, `ConflictDetector`, the DTO contracts, and the ports `IClock` / `IScheduleStore` |
| Frameworks | `Infrastructure` | Application | `AppDbContext`, migrations, `ScheduleStore` (EF Core), `PinnedClock`, the CSV seeder |
| Composition | `Api` | Application, Infrastructure | Minimal API endpoints, the static board, `Program.cs` |

`Tests/ArchitectureTests` makes the rule enforceable: it fails if `Application`
ever references EF Core or ASP.NET.

### The persistence port

The use-case services previously took `AppDbContext` directly. They now depend
on **`IScheduleStore`**, an interface owned by `Application` with
intention-revealing methods (`GetBookingsForDayAsync`, `RecordMoveAsync`, …)
that return materialised domain objects — no `IQueryable` crosses the boundary.
`Infrastructure` supplies the one EF Core implementation.

### Trade-off against the brief

`CLAUDE.md` says *"don't build a repository interface unless tests need it"*,
and by that yardstick `IScheduleStore` is more than the feature strictly
requires — the SQLite-backed tests worked fine against a concrete `DbContext`.
It is here deliberately, to make the dependency rule real rather than a
convention, at the cost of one interface and one implementation class. If the
goal were minimal footage for the reschedule feature alone, the services would
keep taking `AppDbContext` and the solution would stay a single project with
folders.
