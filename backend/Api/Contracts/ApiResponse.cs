using TutoringScheduling.Application.Contracts;

namespace TutoringScheduling.Api.Contracts;

/// <summary>
/// Shared success/error envelope every endpoint responds with, so the
/// frontend has one wire shape to parse instead of one per endpoint.
/// </summary>
public sealed record ApiResponse<T>(T? Data, ApiError? Error)
{
    public static ApiResponse<T> Ok(T data) => new(data, null);

    public static ApiResponse<T> Fail(ApiError error) => new(default, error);
}

public record ApiError(string Code, string Message);

/// <summary>
/// A conflict error that also carries the clashing conflicts, used only by
/// the move-lesson endpoint's 409 response (mirrors the previous
/// <c>MoveRejectedResponse</c> contract).
/// </summary>
public sealed record ApiConflictError(string Code, string Message, IReadOnlyList<ConflictDto> Conflicts)
    : ApiError(Code, Message);
