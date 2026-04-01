using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using TransportApi.Data;
using TransportApi.DTOs;

namespace TransportApi.Services;

public class StopService(TransportDbContext db, IMemoryCache cache) : IStopService
{
    private readonly TransportDbContext _db = db;
    private readonly IMemoryCache _cache = cache;

    public async Task<List<StopDto>> GetStops(string mode)
    {
        var cacheKey = $"stops-{mode}";
        _cache.TryGetValue(cacheKey, out List<StopDto>? stops);

        if (stops != null) return stops;

        stops = await _db.Stops
            .Where(s => s.Mode == mode)
            .Select(s => new StopDto
            {
                Id = s.Id,
                Name = s.Name,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                LocationType = s.LocationType,
                ParentStationId = s.ParentStationId,
                WheelchairBoarding = s.WheelchairBoarding,
                PlatformCode = s.PlatformCode,
                Mode = s.Mode
            })
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

        _cache.Set(cacheKey, stops, cacheOptions);

        return stops;
    }
}
