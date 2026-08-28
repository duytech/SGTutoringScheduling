# DECISIONS

## 1. Questions I would ask the owner first

- Is the "exam pair" case an accepted product rule or an informal exception Mai makes manually?
  - If accepted, I should model a lesson slot that can contain more than one student under one tutor.
  - If not accepted, I should treat those rows as evidence of existing operational workarounds and prevent them for new changes.


- Should historical imported data be preserved exactly even when it breaks the rules, or should import reject bad rows?
  - I assume preserve it exactly, because the brief says the export is what the centre actually ran.
  - If import should reject invalid rows, I would need a different ingestion path and a repair workflow.
  
- For a cancelled and no-show booking, do theirs slot become immediately available for reuse?
  - I assume yes.
  - If not, I re-calculate the tutor load.


## 2. Where the brief does not fully hold together, and how I read it

- The brief says lessons are one-to-one, but the receptionist describes putting two students with one tutor in the same room and same slot on purpose during exam season.
  - My reading: "one-to-one" is the normal mode, not a universal invariant.


## 3. Assumptions I had to invent

- I treat the export as historical truth and load it as seed data without "fixing" it.

- I assume a room can host only one ordinary lesson at a time, unless the owner later confirms that paired lessons are a supported concept rather than historical noise.

- Only booked lessons count in tutor load . Not Cancel and no-show.

## 4. Features I can see this tool needing

- A "today" schedule board that can be understood at a glance.
- Conflict detection for student, tutor, and room overlaps.
- Clear visibility of changes made after the daily cut-off.
- A cancellation workflow that distinguishes cancelled from no-show.
- Booking change (move/reschedule) history.
- Tutor load warnings and enforcement.


## 5. The one feature I chose

- I chose Validate a new booking. The owner specifically described a student being booked into two places at once as something the system must prevent.


## 6. Data model for this feature


### Tables

- `tutors`
  - `id`
  - `name`
  - `subject`
  - `phone`

- `bookings`
  - `id`
  - `student_name`
  - `lesson_date`
  - `start_time`
  - `duration_min`
  - `tutor_id`
  - `room`
  - `status` (`booked`, `cancelled`, `no_show`)
  - `created_at`

## 7. How I represent a booking cancelled or moved after the tutor was told

- Will implement booking change history later.

## 8. Which rules I enforce in the database and which in code

### In the database

I would enforce rules that are structural and unlikely to have business exceptions:

- foreign key from `bookings.tutor_id` to `tutors.id`
- required fields for date, start time, student, tutor, and room


### In code

I would enforce rules that clearly need business interpretation:

the lesson is not on Monday;
the tutor has no overlapping booked lesson;
the student has no overlapping booked lesson;
the room has no overlapping booked lesson;
the tutor would not exceed six booked lessons that day.

A booking is accepted only when all applicable rules pass.

Why:

- The seed data already contains historical rule breaches and apparent exceptions.
- Hard-coding every operational rule into the database would make import and future exceptions awkward.
- The code layer is the right place to apply policy while still preserving historical fact.


## 9. API shape

### Endpoints

- POST /api/bookings/validate
  
## 10. One endpoint I rejected


## 11. Reflection

If I had another week, I would build:

- save booking if valid
- cancellation workflow
- booking change history

## 12 What I know is weak:


## 13 Where my AI assistant helped:

- summarising the brief quickly
- checking the seed data for conflict
- giving ideas for DECISIONS.md
- coding

## 14 One suggestion I threw away, and why I was right to:

- I considered enforcing all rules as hard database uniqueness constraints.
- I rejected that because the brief explicitly says the export is what really happened, not what should have happened.