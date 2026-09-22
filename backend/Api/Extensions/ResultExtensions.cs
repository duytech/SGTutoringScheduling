using TutoringScheduling.Api.Contracts;
using TutoringScheduling.Application.Common;

namespace TutoringScheduling.Api.Extensions;

/// <summary>
/// Maps a use case's <see cref="Result{T}"/> onto the shared HTTP envelope.
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
        var envelope = ApiResponse<T>.Fail(new ApiError(error.Code, error.Message));

        return error.Type switch
        {
            ErrorType.NotFound => Results.Json(envelope, statusCode: StatusCodes.Status404NotFound),
            ErrorType.Validation => Results.Json(envelope, statusCode: StatusCodes.Status400BadRequest),
            ErrorType.Conflict => Results.Json(envelope, statusCode: StatusCodes.Status409Conflict),
            _ => Results.Json(envelope, statusCode: StatusCodes.Status500InternalServerError),
        };
    }
}
