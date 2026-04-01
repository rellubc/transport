using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using TransportStatic.Data;
using TransportStatic.DTOs;

namespace TransportStatic.Services;

public class StopService(TransportDbContext db, IMemoryCache cache) : IStopService
{
    private readonly TransportDbContext _db = db;
    private readonly IMemoryCache _cache = cache;

    public async Task<List<StopDTO>> GetStops(string mode)
    {
        var cacheKey = $"stops-{mode}";
        _cache.TryGetValue(cacheKey, out List<StopDTO>? stops);

        if (stops != null) return stops;

        stops = await _db.Stops
            .Where(s => s.Mode == mode)
            .Select(s => new StopDTO
            {
                Id = s.Id,
                Name = s.Name,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                ParentStationId = s.ParentStationId,
                WheelchairBoarding = s.WheelchairBoarding,
                PlatformCode = s.PlatformCode
            })
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

        _cache.Set(cacheKey, stops, cacheOptions);

        return stops;
    }
}
