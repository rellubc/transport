using Microsoft.EntityFrameworkCore;

using TransportStatic.Data;
using TransportStatic.DTOs;

namespace TransportStatic.Services;

public class TripService(TransportDbContext db) : ITripService
{
    private readonly TransportDbContext _db = db;

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = await _db.Trips
            .Select(t => new TripDTO
            {
                Id = t.Id,
                RouteId = t.RouteId,
                ServiceId = t.ServiceId,
                ShapeId = t.ShapeId,
                HeadSign = t.HeadSign,
                DirectionId = t.DirectionId,
                ShortName = t.ShortName,
                BlockId = t.BlockId,
                WheelchairAccessible = t.WheelchairAccessible,
                TripNote = t.TripNote,
                RouteDirection = t.RouteDirection,
                BikesAllowed = t.BikesAllowed,
            })
            .ToListAsync();

        return trips;
    }

    public async Task<TripDTO?> GetTrip(string tripId)
    {
        var trip = await _db.Trips
            .Where(t => t.Id == tripId)
            .Select(t => new TripDTO
            {
                Id = t.Id,
                RouteId = t.RouteId,
                ServiceId = t.ServiceId,
                ShapeId = t.ShapeId,
                HeadSign = t.HeadSign,
                DirectionId = t.DirectionId,
                ShortName = t.ShortName,
                BlockId = t.BlockId,
                WheelchairAccessible = t.WheelchairAccessible,
                TripNote = t.TripNote,
                RouteDirection = t.RouteDirection,
                BikesAllowed = t.BikesAllowed,
            })
            .FirstOrDefaultAsync();

        return trip;
    }
}
