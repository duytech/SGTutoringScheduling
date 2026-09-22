using Microsoft.Data.SqlClient;
using TutoringScheduling.Application.Abstractions;

namespace TutoringScheduling.Infrastructure.Persistence;

/// <summary>
/// Guards the three DB touchpoints inside <c>MoveLessonService</c>'s
/// Serializable transaction: every <see cref="SqlException"/> found anywhere
/// in an exception's chain is translated into an Application-owned
/// exception, so a raw provider exception never crosses the Infrastructure
/// boundary. SQL Server error 1205 (deadlock victim) and 3960 (snapshot/
/// serialization conflict) — the two ways a Serializable transaction can
/// lose a race against a concurrent writer — become
/// <see cref="ConcurrentWriteConflictException"/>, which the caller can act
/// on (retry/reject). Every other SQL error becomes the generic
/// <see cref="DataAccessException"/>.
/// </summary>
internal static class SqlErrorTranslator
{
    // SQL Server error numbers: 1205 = deadlock victim, 3960 = snapshot/serialization conflict.
    private static readonly HashSet<int> SerializationFailureErrorNumbers = [1205, 3960];

    public static async Task<T> TranslateAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (Find(ex) is { } sqlException)
        {
            throw Translate(sqlException);
        }
    }

    public static async Task TranslateAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex) when (Find(ex) is { } sqlException)
        {
            throw Translate(sqlException);
        }
    }

    private static Exception Translate(SqlException sqlException) =>
        SerializationFailureErrorNumbers.Contains(sqlException.Number)
            ? new ConcurrentWriteConflictException(
                "The slot was taken by another request at the same time.", sqlException)
            : new DataAccessException(
                "A database error occurred while processing the move.", sqlException);

    private static SqlException? Find(Exception? ex)
    {
        for (; ex is not null; ex = ex.InnerException)
        {
            if (ex is SqlException sqlException)
            {
                return sqlException;
            }
        }

        return null;
    }
}
