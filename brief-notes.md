# Brief notes (reference — not auto-loaded, read when needed)

## Grading emphasis (from the assignment sheet)
Order of weight: problem reading > prioritisation > data model > API/code >
communication & reflection. Code correctness matters less than picking the
right one feature and defending the choice.

## Contradictions in the brief (raise with the owner; also go in DECISIONS.md)
1. Owner: "if the system allows [double-booking], the system is broken."
   Receptionist: deliberately double-books one tutor/room/slot weekly for
   "exam pairs," calls it a feature families like. Direct contradiction —
   resolve by treating exam pairs as an explicit, separate action (a
   `lesson_groups` / pairing endpoint), and everything else duplicate as a
   hard reject.
2. "No tutor >6 bookings/day" stated as a rule, but the README says the
   receptionist already breaks it "when desperate" and the owner still wants
   it "enforced." The seed data contains a real violation (see below) — a
   hard DB constraint would make the seed data fail to load. Treat as a
   code-level warning, not a rejection, unless the owner says otherwise.
3. The 4-hour late-cancellation fee rule is written only for a *family*
   cancelling. The seed data has a same-day *tutor* cancellation (illness).
   The rule doesn't say whether the family fee applies when the tutor is at
   fault. Left as an open question / manual-review flag.
4. "READ FIRST" note about a custom typeface / no copy-paste: read the PDF
   pages visually, don't trust copy-pasted text from it.

## Real conflicts found in seed-data/lessons_export.csv (35 rows)
- **True physical double-booking**: tutor T1 booked in room R1 (L033) and
  room R2 (L034) at the same time (2026-03-10, 09:00). Matches the owner's
  "student booked into two places, nobody knew" story — impossible, must be
  blocked unconditionally.
- **Same student, two different tutors, same time**: "Le Minh Chau" booked
  with T3 (L007) and T2 (L008) both at 09:00 on 2026-03-04. A different
  conflict class from tutor/room collision — worth modeling separately if
  in scope.
- **Deliberate exam pair**: L009 & L010 — same tutor T1, same room R1, same
  time 11:00 on 2026-03-04, two different students, note "exam pair - half
  price." This is the sanctioned exception from contradiction #1 — must NOT
  be flagged as an error.
- **Daily load limit violated**: tutor T1 has 7 bookings on 2026-03-06
  (L018, L021, L022, L024, L025, L026, L027) — over the stated 6/day limit.
  Confirms contradiction #2 is real in the data, not hypothetical.
- **Ambiguous late cancellation**: L017 cancelled at 14:40 for a 16:00 lesson
  same day (2026-03-05), note "tutor sick" — inside the 4-hour window, but
  it's the tutor cancelling, not the family. Rule as written doesn't cover
  this case (contradiction #3).
- **No-show vs cancelled**: L015 is `no_show` — per the brief this must NOT
  free the room/slot, unlike a `cancelled` row which does.
- **Untracked history**: L032 note says "moved from Sunday at the family's
  request," but no row shows the original Sunday booking — the export
  itself doesn't track move history. Relevant if building move/reschedule:
  design an append-only event log (`lesson_events`) rather than in-place
  updates, so a moved/cancelled booking's prior state isn't lost.

## Candidate features considered (pick one, argue for it, in DECISIONS.md)
- Validate a new booking (reject/flag conflicts at creation time)
- Read-only "today" dashboard, grouped by room — the one thing the owner
  explicitly quoted wanting ("open the laptop and see today, not scroll")
- Cancellation handling with fee computation
- Move/reschedule with post-cutoff change tracking
- Tutor-facing mobile schedule view
- Bulk import/export tooling for the receptionist

## UI guidance if the chosen feature needs one
Owner's one directly-quoted request is a visual "see today" view — so some
UI should exist even if the feature itself is backend-heavy, but keep it to
a single static HTML page hitting the API with vanilla JS (no SPA build
step). Read-only is enough; no forms/CRUD in the UI unless the feature
requires it. A conflict/warning banner at the top of the page is more
important than styling the grid.

## Data model sketch (adjust per chosen feature — don't build tables the
chosen feature doesn't need)
Tables: tutors, rooms, students, lesson_groups (exam pairs), lessons,
lesson_events (append-only; only needed if move/cancel is in scope).
Physical impossibilities (tutor/room double-booking without a shared
group_id) → DB unique constraints. Business-policy rules that the business
itself overrides (daily load limit) → code-level checks, not DB constraints.
