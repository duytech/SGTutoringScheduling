using TutoringScheduling.Api.Contracts;
using TutoringScheduling.Application.Common;

namespace TutoringScheduling.Api.Extensions;

/// <summary>
/// Maps a use case's <see cref="Result{T}"/> onto the shared HTTP envelope.
/// Always HTTP 200 — success or failure is signaled by <c>error.code</c> in
/// the body, not by status code, so the client has one thing to branch on.
/// Endpoints whose failures need extra shape (e.g. move-lesson's conflicts)
/// map themselves instead of using this.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<T>.Ok(result.Value!));
        }

        var error = result.Error!;
        return Results.Ok(ApiResponse<T>.Fail(new ApiError(error.Code, error.Message)));
    }
}
