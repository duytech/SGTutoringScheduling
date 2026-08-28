using System.Text.Json;
using SynergieGlobalTutoringScheduling.Domain;

namespace SynergieGlobalTutoringScheduling.Services;

public sealed class JsonScheduleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataDirectoryPath;

    public JsonScheduleStore(IWebHostEnvironment environment)
    {
        _dataDirectoryPath = Path.Combine(environment.ContentRootPath, "Data");
    }

    public async Task<IReadOnlyList<Booking>> LoadBookingsAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_dataDirectoryPath, "lessons.json");
        await using var stream = File.OpenRead(filePath);

        var bookings = await JsonSerializer.DeserializeAsync<List<Booking>>(stream, JsonOptions, cancellationToken);

        return bookings ?? [];
    }

    public async Task<IReadOnlyList<Tutor>> LoadTutorsAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_dataDirectoryPath, "tutors.json");
        await using var stream = File.OpenRead(filePath);

        var tutors = await JsonSerializer.DeserializeAsync<List<Tutor>>(stream, JsonOptions, cancellationToken);

        return tutors ?? [];
    }
}
