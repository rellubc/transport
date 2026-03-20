using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;

namespace TransportApi.Services;

public class TripService(TransportDbContext db) : ITripService
{
    private readonly TransportDbContext _db = db;

    public async Task<List<TripDto>> GetTrips()
    {
        var trips = await _db.Trips
            .Select(t => new TripDto
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

    public async Task<TripDto?> GetTrip(string tripId)
    {
        var trip = await _db.Trips
            .Where(t => t.Id == tripId)
            .Select(t => new TripDto
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
