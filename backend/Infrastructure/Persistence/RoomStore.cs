using Microsoft.EntityFrameworkCore;
using TutoringScheduling.Application.Abstractions;
using TutoringScheduling.Domain;

namespace TutoringScheduling.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IRoomStore"/>. One of the few places a
/// <see cref="AppDbContext"/> is touched on the read path.
/// </summary>
public sealed class RoomStore : IRoomStore
{
    private readonly AppDbContext _db;

    public RoomStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> RoomExistsAsync(string roomId, CancellationToken cancellationToken = default)
    {
        return _db.Rooms.AnyAsync(room => room.Id == roomId, cancellationToken);
    }

    public async Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Rooms
            .OrderBy(room => room.Id)
            .ToListAsync(cancellationToken);
    }
}
