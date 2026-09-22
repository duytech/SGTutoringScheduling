using TutoringScheduling.Application.Common;
using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Application;

/// <summary>
/// A move rejection that carries the blocking conflicts it clashed with, so
/// the move endpoint can still surface them (mirrors the previous
/// <c>MoveRejectedResponse</c> contract) without the shared <see cref="Error"/>
/// type having to know about conflicts.
/// </summary>
public sealed record MoveConflictError(string Message, IReadOnlyList<ConflictDto> Conflicts)
    : Error("move_conflict", Message, ErrorType.Conflict);
