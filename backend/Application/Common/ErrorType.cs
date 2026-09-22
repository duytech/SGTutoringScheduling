namespace TutoringScheduling.Application.Common;

/// <summary>
/// Broad category an <see cref="Error"/> falls into, used by the Api layer to
/// pick an HTTP status code without each endpoint re-deciding it.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
}
