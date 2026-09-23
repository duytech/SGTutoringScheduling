namespace TutoringScheduling.Application.Common;

/// <summary>
/// Why a <see cref="Result"/> or <see cref="Result{T}"/> failed. <see cref="Code"/>
/// is the machine-readable identifier clients branch on. Non-sealed so a use case
/// can attach extra data to a specific failure (see <c>MoveConflictError</c>)
/// while everything else rides on this shape.
/// </summary>
public record Error(string Code, string Message)
{
    public static Error Validation(string code, string message) => new(code, message);

    public static Error NotFound(string code, string message) => new(code, message);

    public static Error Conflict(string code, string message) => new(code, message);

    public static Error Failure(string code, string message) => new(code, message);
}
