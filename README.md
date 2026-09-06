# Synergie Global Tutoring Scheduling

Minimal ASP.NET Core API + a small static room board for **Bright Path Learning
Centre**. The chosen feature is a **conflict detection engine** over the
centre's schedule, surfaced through a read-only **"today" view**.

Stack: ASP.NET Core Minimal API, EF Core, SQLite.

## Run it

```bash
dotnet run
```

On start the app applies migrations, creates `synergie.db`, and seeds it from
the CSV export under `seed-data/`. ASP.NET Core prints the local URL; open it
in a browser for the room board, or call the API directly.

```bash
dotnet test
```

runs the conflict-engine unit tests and the seed-data assertions.

### "Today"

The brief pins "today" to a date inside the seeded week, not the real clock.
The default is **2026-03-06** (`Schedule:Today` in `appsettings.json`). The
board and `GET /api/schedule` use it when no date is given.

## Seed data

`seed-data/lessons_export.csv` (35 rows, 2026-03-03 … 2026-03-10) and
`seed-data/tutors.csv` are loaded verbatim, historical conflicts included.
Delete `synergie.db` to re-seed.

## Endpoints

### `GET /api/schedule?date=YYYY-MM-DD`

One day, grouped by room (all six rooms, empty ones included). Each lesson
carries the conflict codes it is part of; the response also has a per-tutor
load line and the day's conflicts. `date` defaults to the pinned today.

### `GET /api/conflicts?from=YYYY-MM-DD&to=YYYY-MM-DD`

Runs the engine over stored bookings (optionally within a date range) and
returns every clash plus an error/warning count.

```json
{
  "from": null,
  "to": null,
  "summary": { "errors": 2, "warnings": 2 },
  "conflicts": [
    {
      "code": "TUTOR_DOUBLE_BOOKED",
      "severity": "error",
      "date": "2026-03-10",
      "message": "Tutor 'T1' is booked for two overlapping lessons on 2026-03-10 09:00.",
      "bookingIds": ["L033", "L034"],
      "tutorId": "T1"
    }
  ]
}
```

### `POST /api/bookings/validate`

Checks a *proposed* booking before it is created (duration, Monday closure,
tutor exists, student/tutor/room overlap, tutor daily limit).

```json
{
  "studentName": "New Student",
  "tutorId": "T2",
  "room": "R4",
  "lessonDate": "2026-03-07",
  "startTime": "13:00:00",
  "durationMinutes": 60
}
```

## Conflict codes

| Code | Severity | Meaning |
| --- | --- | --- |
| `TUTOR_DOUBLE_BOOKED` | error | Same tutor in two overlapping lessons |
| `ROOM_DOUBLE_BOOKED` | error | Same room, two overlapping lessons, not an exam pair |
| `STUDENT_DOUBLE_BOOKED` | error | Same student in two overlapping lessons |
| `TUTOR_DAILY_LIMIT` | warning | A tutor is over the 6-lessons-a-day limit |
| `CENTRE_CLOSED_MONDAY` | warning | A lesson is scheduled on a Monday |

Cancelled bookings are ignored (the slot is free); no-shows still occupy the
slot. Lessons that share a `GroupId` are a sanctioned exam pair and do not
clash with each other.

The seed export contains exactly two errors (`L033`/`L034` tutor double-booked,
`L007`/`L008` student with two tutors) and two warnings (tutor `T1` over limit
on 2026-03-06, one Monday lesson `L032`).
