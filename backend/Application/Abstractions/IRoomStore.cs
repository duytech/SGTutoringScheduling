using TutoringScheduling.Domain;

namespace TutoringScheduling.Application.Abstractions;

/// <summary>
/// Read access to rooms. Infrastructure supplies the EF Core implementation;
/// the application never sees a <c>DbContext</c>.
/// </summary>
public interface IRoomStore
{
    Task<bool> RoomExistsAsync(string roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken = default);
}
