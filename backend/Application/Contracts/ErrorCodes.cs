namespace TutoringScheduling.Application.Contracts;

/// <summary>
/// Values of <c>error.code</c> in API responses. Clients branch on these, so
/// the string values are part of the API contract.
/// </summary>
public static class ErrorCodes
{
    /// <summary>No lesson exists with the requested id.</summary>
    public const string LessonNotFound = "lesson_not_found";

    /// <summary>The move request is missing the target date or start time.</summary>
    public const string MoveInvalidRequest = "move_invalid_request";

    /// <summary>The lesson's status (e.g. cancelled) does not allow it to be moved.</summary>
    public const string MoveNotBookable = "move_not_bookable";

    /// <summary>The lesson is half of an exam pair, which must be moved together.</summary>
    public const string MovePairedLesson = "move_paired_lesson";

    /// <summary>The target date, time and room are the lesson's current ones.</summary>
    public const string MoveNoChange = "move_no_change";

    /// <summary>The centre is closed on the target date.</summary>
    public const string MoveCentreClosed = "move_centre_closed";

    /// <summary>The target room does not exist.</summary>
    public const string MoveRoomNotFound = "move_room_not_found";

    /// <summary>The target slot clashes with existing lessons; see the attached conflicts.</summary>
    public const string MoveConflict = "move_conflict";

    /// <summary>A concurrent request took the slot first; the move can be retried.</summary>
    public const string MoveRetry = "move_retry";
}
