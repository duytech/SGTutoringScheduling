using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SynergieGlobalTutoringScheduling.Domain;

namespace SynergieGlobalTutoringScheduling.Data;

/// <summary>
/// Loads the centre's schedule from the seed export the first time the
/// database is empty. The export is treated as historical truth: rows that
/// break current booking rules are inserted exactly as they are.
/// </summary>
public static class DbSeeder
{
    private const int RoomCount = 6;

    /// <summary>
    /// Lessons that share a tutor, room and slot on purpose ("exam pair -
    /// half price"). The seed CSV has no group column, so the pairing is
    /// applied here from a known id set rather than inferred from the note
    /// text. See brief-notes.md, "Real conflicts found in seed data".
    /// </summary>
    private static readonly Dictionary<string, string> ExamPairGroupByLessonId = new()
    {
        ["L009"] = "G1",
        ["L010"] = "G1",
    };

    public static async Task SeedAsync(
        AppDbContext db,
        string contentRootPath,
        CancellationToken cancellationToken = default)
    {
        if (await db.Bookings.AnyAsync(cancellationToken))
        {
            return;
        }

        var seedDirectory = Path.Combine(contentRootPath, "seed-data");

        var rooms = BuildRooms();
        var tutors = ReadTutors(Path.Combine(seedDirectory, "tutors.csv"));
        var bookings = ReadBookings(Path.Combine(seedDirectory, "lessons_export.csv"));

        db.Rooms.AddRange(rooms);
        db.Tutors.AddRange(tutors);
        db.Bookings.AddRange(bookings);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static List<Room> BuildRooms()
    {
        return Enumerable.Range(1, RoomCount)
            .Select(number => new Room { Id = $"R{number}", Name = $"Room {number}" })
            .ToList();
    }

    internal static List<Tutor> ReadTutors(string filePath)
    {
        return ReadDataRows(filePath)
            .Select(fields => new Tutor
            {
                Id = fields[0],
                Name = fields[1],
                Subject = fields[2],
                Phone = fields[3],
            })
            .ToList();
    }

    internal static List<Booking> ReadBookings(string filePath)
    {
        var bookings = new List<Booking>();

        foreach (var fields in ReadDataRows(filePath))
        {
            var id = fields[0];
            var cancelledAtRaw = fields[8];
            var note = fields[9];

            bookings.Add(new Booking
            {
                Id = id,
                LessonDate = DateOnly.ParseExact(fields[1], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                StartTime = TimeOnly.ParseExact(fields[2], "HH:mm", CultureInfo.InvariantCulture),
                DurationMinutes = int.Parse(fields[3], CultureInfo.InvariantCulture),
                StudentName = fields[4],
                TutorId = fields[5],
                RoomId = fields[6],
                Status = ParseStatus(fields[7]),
                CancelledAt = string.IsNullOrWhiteSpace(cancelledAtRaw)
                    ? null
                    : DateTimeOffset.Parse(cancelledAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Note = string.IsNullOrWhiteSpace(note) ? null : note,
                GroupId = ExamPairGroupByLessonId.GetValueOrDefault(id),
            });
        }

        return bookings;
    }

    private static BookingStatus ParseStatus(string raw) => raw.Trim().ToLowerInvariant() switch
    {
        "booked" => BookingStatus.Booked,
        "cancelled" => BookingStatus.Cancelled,
        "no_show" => BookingStatus.NoShow,
        _ => throw new FormatException($"Unknown booking status '{raw}' in seed data."),
    };

    /// <summary>
    /// Yields the data rows of a simple comma-separated export (header skipped).
    /// The seed files have no quoted fields or embedded commas; only the final
    /// column may contain other punctuation, so the split is capped at the
    /// known column count.
    /// </summary>
    private static IEnumerable<string[]> ReadDataRows(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        if (lines.Length == 0)
        {
            yield break;
        }

        var columnCount = lines[0].Split(',').Length;

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            yield return line.Split(',', columnCount);
        }
    }
}
