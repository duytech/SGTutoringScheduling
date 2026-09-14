namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// The current instant, as the application layer sees it. "Now" is pinned to a
/// value inside the seeded week by the implementation, never the real system
/// clock - the brief requires it.
///
/// This is the Clock/TimeProvider abstraction pattern - dependency injection
/// applied to ambient time, analogous to .NET's built-in <see cref="TimeProvider"/>.
/// Consumers depend on this interface instead of calling
/// <see cref="DateTimeOffset.UtcNow"/> directly, which buys determinism and
/// testability: <c>PinnedClock</c> pins "now" to the seeded week for the running
/// app, while <c>FixedClock</c> lets tests set arbitrary instants to exercise
/// time-dependent rules (the 4h free-cancellation window, the 16:00-cutoff
/// visibility rule) without depending on the real system clock.
/// </summary>
public interface IClock
{
    DateTimeOffset Now { get; }

    DateOnly Today => DateOnly.FromDateTime(Now.DateTime);
}
