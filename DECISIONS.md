# DECISIONS

## 1. Questions I would ask the owner first

- Is the "exam pair" an accepted product rule or an informal exception Mai
  makes by hand?
  - I assume it is accepted, so I model it as a real thing: lessons that
    share a `group_id`, created by an explicit pairing action. The engine
    then treats a pair as one occupancy block and never flags it.
  - If it is not accepted, those rows are historical workarounds and I would
    flag them like any other duplicate.

- Should the historical export be preserved exactly even where it breaks the
  rules, or should import reject bad rows?
  - I assume preserve exactly. The brief says the export is what the centre
    actually ran, so the database loads it as-is and the engine reports the
    breaches on top.

- Does a cancelled or no-show booking free the slot?
  - I assume **cancelled frees** the slot and **no-show does not** (matches
    the brief's fee rules). So the engine ignores cancelled bookings and
    keeps no-shows.

- New open questions from building this:
  - Does an exam pair count as **one or two** lessons in a tutor's daily
    load? I count two.
  - `L017` is a same-day cancellation by the *tutor* (illness), inside the
    4-hour window written for *families*. Whose rule applies? Left as a
    manual-review question; the engine just treats it as cancelled.
  - Lessons run to 20:30 in the seed (`L027`). Are evening slots normal, or
    should late lessons be flagged? Not flagged for now.

## 2. Where the brief does not fully hold together

- Owner: "if the system allows double-booking, the system is broken."
  Receptionist: deliberately double-books one tutor/room/slot for exam pairs.
  - My reading: exam pairs are a **separate, explicit concept** (`group_id`),
    everything else that collides is a real conflict.

- "No tutor more than 6 bookings a day" is stated as a rule, but the business
  breaks it and the seed data contains a real 7-booking day (`T1`,
  2026-03-06).
  - My reading: it is a **warning**, not a hard block. The engine reports it;
    nothing rejects the data.

## 3. Assumptions I had to invent

- The export is historical truth. It loads without being "fixed".
- A room hosts one lesson at a time unless the lessons are a confirmed exam
  pair.
- Only non-cancelled lessons count toward tutor load (booked + no-show).
- The seed CSV has no group column, so I assign the exam-pair `group_id` at
  import time from the known lesson-id set (`L009`, `L010`), not by reading
  the note text.

## 4. Features I can see this tool needing

- A "today" board that can be read at a glance.
- Conflict detection for student, tutor, and room overlaps.
- Visibility of changes made after the daily cut-off.
- A cancellation workflow that separates cancelled from no-show.
- Booking move/reschedule history.
- Tutor load warnings and enforcement.

## 5. The one feature I chose

**A conflict detection engine, plus a read-only "today" view that surfaces it.**

The owner's strongest quote is about a student booked into two places with
nobody knowing. The seed data proves this is real, not hypothetical: a tutor
in two rooms at once, a student with two tutors at once, a tutor over the
daily limit. The owner also explicitly asked to "open the laptop and see
today". One feature covers both: an engine that classifies every clash in the
schedule, and a board that shows today's rooms with those clashes called out
at the top.

This is different from validating a *new* booking (which the repo already
had): the engine looks at the data that already exists.

## 6. Data model for this feature

EF Core over SQLite. Three tables.

- `Tutors` — `Id`, `Name`, `Subject`, `Phone`
- `Rooms` — `Id`, `Name` (R1…R6; a table, not a constant, so the board can
  show empty rooms and the FK is real)
- `Bookings` — `Id`, `StudentName`, `LessonDate`, `StartTime`,
  `DurationMinutes`, `TutorId` → `Tutors`, `RoomId` → `Rooms`, `Status`
  (`Booked` / `Cancelled` / `NoShow`), `GroupId` (nullable — exam pairs),
  `CancelledAt`, `Note`

No `lesson_events` / move-history table: reschedule tracking is out of scope
for this feature, so I did not build tables it does not use. `GroupId` is a
plain nullable string; a `lesson_groups` table would only be worth it once
pairing has its own attributes (price, who authorised it).

## 7. How I represent a booking cancelled or moved after the tutor was told

Not in scope for this feature. If I built it, it would be an append-only
`lesson_events` log rather than in-place edits, so the state a tutor was last
told is never overwritten. `L032` ("moved from Sunday") shows the export
itself does not keep this.

## 8. Which rules I enforce in the database and which in code

### In the database (schema + migration)

- foreign keys `Bookings.TutorId` → `Tutors`, `Bookings.RoomId` → `Rooms`
- required columns: student, date, start time, duration, tutor, room, status
- non-unique indexes on `(TutorId, LessonDate)` and `(RoomId, LessonDate)`

### In code (the engine)

- tutor / room / student overlap
- exam-pair exclusion (shared `GroupId`)
- tutor daily limit (> 6)
- Monday closure

### Why

Even the "physical" rules cannot be database constraints here: the seed
export already violates them (`L033`/`L034`), so a unique index would make
the import fail. Overlap is interval math, not a uniqueness check. The daily
limit is a policy the business itself overrides. So the database guarantees
referential integrity and required fields; the engine detects everything that
needs business judgement, on data the database is required to accept.

## 9. API shape

- `GET /api/schedule?date=` — one day, grouped by room, with per-lesson
  conflict codes, tutor load, and the day's conflicts. Defaults to the pinned
  "today" (2026-03-06).
- `GET /api/conflicts?from=&to=` — the engine over all stored bookings (or a
  date range), with an error/warning count.
- `POST /api/bookings/validate` — pre-existing; checks a proposed booking.

One static page at `/` calls `GET /api/schedule` and renders the board with a
conflict banner on top. No SPA build, read-only, no forms.

## 10. One endpoint I rejected

A generic `PUT /lessons/{id}`. The brief calls this out, and it would let a
move, a cancellation and a room change all look identical in the data. When
those land they should be separate intent endpoints
(`/cancel`, `/move`, `/create-pair`) writing to an event log.

## 11. Reflection

With another week I would build, in order:

- persist a valid booking (the validate endpoint currently only checks)
- cancellation workflow with the 4-hour fee rule and the tutor-fault case
- move/reschedule with an append-only event log and post-16:00 change
  visibility
- let the board filter to one tutor (the tutor-facing view)

## 12. What I know is weak

- Overlap detection is O(n²) per day. Fine for one centre's week; not for
  years of history. It would move to an indexed range query or a sweep line.
- Exam-pair grouping is seeded from a hard-coded id set. Real pairing needs
  its own endpoint and probably its own table.
- The board's grid is deliberately plain — I spent the styling budget on the
  conflict banner, per the brief's steer.
- No end-to-end test of the HTTP endpoints; the engine and the read service
  are covered, the wiring is only checked by hand.

## 13. Where my AI assistant helped

- summarising the brief and its contradictions
- cross-checking the seed data for every conflict class
- scaffolding the EF layer, the migration and the test project
- drafting this document

## 14. One suggestion I threw away, and why I was right to

Enforcing every rule as a hard database constraint (unique indexes on
tutor/room/slot, a check on the daily limit). It would have been less code,
but the seed export — which the brief says is what really happened — would
not load. The whole point of the feature is to report on data that breaks the
rules, so the rules cannot live where they would reject that data.
