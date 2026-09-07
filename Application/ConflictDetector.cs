using TutoringScheduling.Application.Contracts;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Application;

/// <summary>
/// Scans a set of bookings and reports every clash. Pure function, no data
/// access: the caller decides which slice of the schedule to feed in.
///
/// Cancelled bookings are ignored (they free the slot). No-shows are kept
/// (they do not). Two bookings that share a <see cref="Booking.GroupId"/> are
/// a sanctioned exam pair and never clash with each other.
/// </summary>
public static class ConflictDetector
{
    public static IReadOnlyList<ConflictDto> Detect(IReadOnlyList<Booking> bookings)
    {
        var active = bookings
            .Where(booking => booking.Status != BookingStatus.Cancelled)
            .ToList();

        var conflicts = new List<ConflictDto>();
        conflicts.AddRange(FindOverlapConflicts(active));
        conflicts.AddRange(FindDailyLimitConflicts(active));
        conflicts.AddRange(FindMondayConflicts(active));

        return conflicts
            .OrderByDescending(conflict => conflict.Severity == ConflictSeverities.Error)
            .ThenBy(conflict => conflict.Date)
            .ThenBy(conflict => conflict.Code, StringComparer.Ordinal)
            .ThenBy(conflict => string.Join(',', conflict.BookingIds), StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<ConflictDto> FindOverlapConflicts(IReadOnlyList<Booking> active)
    {
        for (var i = 0; i < active.Count; i++)
        {
            for (var j = i + 1; j < active.Count; j++)
            {
                var first = active[i];
                var second = active[j];

                if (!Overlaps(first, second))
                {
                    continue;
                }

                if (IsSameExamPair(first, second))
                {
                    continue;
                }

                var when = $"{first.LessonDate:yyyy-MM-dd} {Earliest(first, second):HH\\:mm}";

                if (first.TutorId == second.TutorId)
                {
                    yield return Pairwise(
                        ConflictCodes.TutorDoubleBooked,
                        first, second,
                        $"Tutor '{first.TutorId}' is booked for two overlapping lessons on {when}.",
                        tutorId: first.TutorId);
                }

                if (first.RoomId == second.RoomId)
                {
                    yield return Pairwise(
                        ConflictCodes.RoomDoubleBooked,
                        first, second,
                        $"Room '{first.RoomId}' holds two overlapping lessons on {when}.",
                        roomId: first.RoomId);
                }

                if (string.Equals(first.StudentName, second.StudentName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return Pairwise(
                        ConflictCodes.StudentDoubleBooked,
                        first, second,
                        $"Student '{first.StudentName}' is booked for two overlapping lessons on {when}.",
                        studentName: first.StudentName);
                }
            }
        }
    }

    private static IEnumerable<ConflictDto> FindDailyLimitConflicts(IReadOnlyList<Booking> active)
    {
        return active
            .GroupBy(booking => (booking.TutorId, booking.LessonDate))
            .Where(group => group.Count() > BookingLimits.MaxBookingsPerTutorPerDay)
            .Select(group => new ConflictDto
            {
                Code = ConflictCodes.TutorDailyLimit,
                Severity = ConflictSeverities.Warning,
                Date = group.Key.LessonDate,
                Message =
                    $"Tutor '{group.Key.TutorId}' has {group.Count()} lessons on " +
                    $"{group.Key.LessonDate:yyyy-MM-dd}, over the daily limit of " +
                    $"{BookingLimits.MaxBookingsPerTutorPerDay}.",
                BookingIds = OrderedIds(group),
                TutorId = group.Key.TutorId,
            });
    }

    private static IEnumerable<ConflictDto> FindMondayConflicts(IReadOnlyList<Booking> active)
    {
        return active
            .Where(booking => booking.LessonDate.DayOfWeek == DayOfWeek.Monday)
            .GroupBy(booking => booking.LessonDate)
            .Select(group => new ConflictDto
            {
                Code = ConflictCodes.CentreClosedMonday,
                Severity = ConflictSeverities.Warning,
                Date = group.Key,
                Message =
                    $"{group.Count()} lesson(s) are scheduled on Monday " +
                    $"{group.Key:yyyy-MM-dd}, when the centre is closed to bookings.",
                BookingIds = OrderedIds(group),
            });
    }

    private static bool Overlaps(Booking first, Booking second)
    {
        return first.LessonDate == second.LessonDate
            && first.GetStartDateTime() < second.GetEndDateTime()
            && second.GetStartDateTime() < first.GetEndDateTime();
    }

    private static bool IsSameExamPair(Booking first, Booking second)
    {
        return first.GroupId is not null && first.GroupId == second.GroupId;
    }

    private static TimeOnly Earliest(Booking first, Booking second)
    {
        return first.StartTime <= second.StartTime ? first.StartTime : second.StartTime;
    }

    private static ConflictDto Pairwise(
        string code,
        Booking first,
        Booking second,
        string message,
        string? tutorId = null,
        string? roomId = null,
        string? studentName = null)
    {
        return new ConflictDto
        {
            Code = code,
            Severity = ConflictSeverities.Error,
            Date = first.LessonDate,
            Message = message,
            BookingIds = new[] { first.Id, second.Id }.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            TutorId = tutorId,
            RoomId = roomId,
            StudentName = studentName,
        };
    }

    private static List<string> OrderedIds(IEnumerable<Booking> bookings)
    {
        return bookings
            .OrderBy(booking => booking.StartTime)
            .ThenBy(booking => booking.Id, StringComparer.Ordinal)
            .Select(booking => booking.Id)
            .ToList();
    }
}
