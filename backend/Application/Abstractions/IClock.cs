namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// The current instant, as the application layer sees it. "Now" is pinned to a
/// value inside the seeded week by the implementation, never the real system
/// clock - the brief requires it.
/// </summary>
public interface IClock
{
    DateTimeOffset Now { get; }

    DateOnly Today => DateOnly.FromDateTime(Now.DateTime);
}
