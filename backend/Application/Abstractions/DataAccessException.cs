namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// An unexpected database failure the caller cannot meaningfully retry or
/// interpret (connection loss, timeout, constraint violation, etc.) — unlike
/// <see cref="ConcurrentWriteConflictException"/>, which the caller can act
/// on. Infrastructure throws this instead of letting a provider-specific
/// exception (e.g. SqlException) cross into the Application layer.
/// </summary>
public sealed class DataAccessException : Exception
{
    public DataAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
