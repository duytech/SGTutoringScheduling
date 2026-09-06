namespace TutoringScheduling.Contracts;

public static class ValidationErrorCodes
{
    public const string InvalidDuration = "INVALID_DURATION";
    public const string CentreClosedMonday = "CENTRE_CLOSED_MONDAY";
    public const string TutorNotFound = "TUTOR_NOT_FOUND";
    public const string StudentOverlap = "STUDENT_OVERLAP";
    public const string TutorOverlap = "TUTOR_OVERLAP";
    public const string RoomOverlap = "ROOM_OVERLAP";
    public const string TutorDailyLimitExceeded = "TUTOR_DAILY_LIMIT_EXCEEDED";
}
