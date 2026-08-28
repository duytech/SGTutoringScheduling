# Synergie Global Tutoring Scheduling

Minimal ASP.NET Core API for validating a new tutoring booking against a seeded schedule stored in JSON files.

## What it validates

- tutor exists
- duration is within the allowed range
- new bookings are not accepted on Monday
- student overlap
- tutor overlap
- room overlap
- tutor daily booking limit

## Seed data

The API reads from:

- `Data/lessons.json`
- `Data/tutors.json`

These files were derived from the supplied CSV export and are loaded at request time by `JsonScheduleStore`.

## Error codes

- `INVALID_DURATION`: duration is not one of the allowed lesson lengths
- `CENTRE_CLOSED_MONDAY`: the requested lesson date is a Monday
- `TUTOR_NOT_FOUND`: the requested tutor id does not exist in the seed data
- `STUDENT_OVERLAP`: the student already has another booking in the same time range
- `TUTOR_OVERLAP`: the tutor already has another booking in the same time range
- `ROOM_OVERLAP`: the room is already occupied in the same time range
- `TUTOR_DAILY_LIMIT_EXCEEDED`: the tutor would exceed the maximum number of bookings for that day

## Run the project

```bash
dotnet restore
dotnet run
```

By default, ASP.NET Core prints the local URL in the terminal when the app starts. Use that base URL for the requests below.

## Validate a booking

Endpoint:

```http
POST /api/bookings/validate
Content-Type: application/json
```

### Example request with conflicts

This request clashes with existing bookings on `2026-03-10 09:00`.

```json
{
  "studentName": "New Student",
  "tutorId": "T1",
  "room": "R1",
  "lessonDate": "2026-03-10",
  "startTime": "09:00:00",
  "durationMinutes": 60
}
```

Example response:

```json
{
  "valid": false,
  "errors": [
    {
      "code": "TUTOR_OVERLAP",
      "message": "Tutor 'T1' already has a booking that overlaps 2026-03-10 09:00.",
      "severity": "error",
      "relatedBookingIds": ["L033", "L034"]
    },
    {
      "code": "ROOM_OVERLAP",
      "message": "Room 'R1' is already occupied at 2026-03-10 09:00.",
      "severity": "error",
      "relatedBookingIds": ["L033"]
    }
  ],
  "warnings": []
}
```

### Example request that exceeds the tutor daily limit

This request targets tutor `T1` on `2026-03-06`, where the seed data already contains 7 active bookings for that tutor.

```json
{
  "studentName": "Another Student",
  "tutorId": "T1",
  "room": "R6",
  "lessonDate": "2026-03-06",
  "startTime": "08:00:00",
  "durationMinutes": 60
}
```

Example response:

```json
{
  "valid": false,
  "errors": [
    {
      "code": "TUTOR_DAILY_LIMIT_EXCEEDED",
      "message": "Tutor 'T1' already has 7 bookings on 2026-03-06, so this request would exceed the daily limit of 6.",
      "severity": "error",
      "relatedBookingIds": ["L018", "L021", "L022", "L024", "L025", "L026", "L027"]
    }
  ],
  "warnings": []
}
```

### Example request that passes

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

Expected response:

```json
{
  "valid": true,
  "errors": [],
  "warnings": []
}
```
