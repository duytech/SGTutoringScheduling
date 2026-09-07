namespace TutoringScheduling.Application;

/// <summary>
/// Result of attempting to move a lesson. Kept here so the outcome vocabulary
/// lives apart from <see cref="MoveLessonService"/>.
/// </summary>
public enum MoveOutcome
{
    Applied,
    LessonNotFound,
    Rejected,
}
