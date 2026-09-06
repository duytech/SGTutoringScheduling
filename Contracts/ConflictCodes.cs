namespace SynergieGlobalTutoringScheduling.Contracts;

public static class ConflictCodes
{
    /// <summary>Same tutor in two overlapping lessons (physically impossible).</summary>
    public const string TutorDoubleBooked = "TUTOR_DOUBLE_BOOKED";

    /// <summary>Same room holding two overlapping lessons that are not an exam pair.</summary>
    public const string RoomDoubleBooked = "ROOM_DOUBLE_BOOKED";

    /// <summary>Same student in two overlapping lessons.</summary>
    public const string StudentDoubleBooked = "STUDENT_DOUBLE_BOOKED";

    /// <summary>A tutor is carrying more than the allowed number of lessons in one day.</summary>
    public const string TutorDailyLimit = "TUTOR_DAILY_LIMIT";

    /// <summary>A lesson is scheduled on a Monday, when the centre is closed to bookings.</summary>
    public const string CentreClosedMonday = "CENTRE_CLOSED_MONDAY";
}
