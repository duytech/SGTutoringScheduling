using Microsoft.Data.SqlClient;
using TutoringScheduling.Application.Abstractions;

namespace TutoringScheduling.Infrastructure.Persistence;

/// <summary>
/// Recognises the SQL Server errors a Serializable transaction raises when it
/// loses a race against a concurrent writer, and translates them into the
/// provider-agnostic <see cref="ConcurrentWriteConflictException"/> the
/// Application layer knows how to handle.
/// </summary>
internal static class SqlSerializationFailure
{
    // 1205 = chosen as deadlock victim, 3960 = snapshot/serialization conflict.
    private static readonly HashSet<int> ErrorNumbers = [1205, 3960];

    public static async Task<T> TranslateAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (Find(ex) is { } sqlException)
        {
            throw new ConcurrentWriteConflictException(
                "The slot was taken by another request at the same time.", sqlException);
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
            throw new ConcurrentWriteConflictException(
                "The slot was taken by another request at the same time.", sqlException);
        }
    }

    private static SqlException? Find(Exception? ex)
    {
        for (; ex is not null; ex = ex.InnerException)
        {
            if (ex is SqlException sqlException && ErrorNumbers.Contains(sqlException.Number))
            {
                return sqlException;
            }
        }

        return null;
    }
}
