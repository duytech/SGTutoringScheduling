namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// Opens a transaction spanning the read-check-write sequence of a use case, so
/// two concurrent callers cannot both observe a free slot and both commit into
/// it. Infrastructure supplies the EF Core implementation; the application
/// never sees a <c>DbContext</c>.
/// </summary>
public interface IUnitOfWork
{
    Task<IUnitOfWorkTransaction> BeginSerializableTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Thrown when the database could not serialize the transaction against a
/// concurrent writer (SQL Server error 1205 deadlock victim or 3960 snapshot
/// conflict) and the caller lost the race.
/// </summary>
public sealed class ConcurrentWriteConflictException : Exception
{
    public ConcurrentWriteConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
