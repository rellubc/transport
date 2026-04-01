using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using TransportStatic.Data;
using TransportStatic.DTOs;

namespace TransportStatic.Services;

public class ShapeService(TransportDbContext db, IMemoryCache cache) : IShapeService
{
    private readonly TransportDbContext _db = db;
    private readonly IMemoryCache _cache = cache;

    public async Task<Dictionary<string, List<ShapeCoordinates>>> GetShapes(string mode)
    {
        var cacheKey = $"shapes-{mode}";
        _cache.TryGetValue(cacheKey, out Dictionary<string, List<ShapeCoordinates>>? shapes);

        if (shapes != null) return shapes;

        shapes = await _db.Shapes
            .Where(s => s.Mode == mode)
            .GroupBy(s => s.Id)
            .ToDictionaryAsync(
                g => g.Key,
                g => g
                    .OrderBy(s => s.Sequence)
                    .Select(s => new ShapeCoordinates
                    {
                        Latitude = s.Latitude,
                        Longitude = s.Longitude
                    })
                    .ToList()
            );

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

        _cache.Set(cacheKey, shapes, cacheOptions);

        return shapes;
    }
}
