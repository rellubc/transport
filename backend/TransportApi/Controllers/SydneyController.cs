using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using TransportApi.Data;
using TransportApi.DTOs;
using TransportApi.DTOs.Realtime;
using TransportApi.Models;
using TransportApi.Services;

using TransitRealtime;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney")]
public class SydneyController : ControllerBase
{
    private readonly TransportDbContext _db;
    private readonly ISydneyService _services;
    private readonly IMemoryCache _cache;

    public SydneyController(TransportDbContext db, ISydneyService services, IMemoryCache cache)
    {
        _db = db;
        _services = services;
        _cache = cache;
    }

    [HttpGet("agencies")]
    public async Task<ActionResult<List<AgencyDto>>> GetSydneyAgencies() =>
        Ok(await _db.Agencies.ToListAsync());

    [HttpGet("calendars")]
    public async Task<ActionResult<List<CalendarDto>>> GetSydneyCalendars(string? serviceId)
    {   
        IQueryable<Models.Calendar> query = _db.Calendars;

        if (!string.IsNullOrWhiteSpace(serviceId))
        {
            query = query.Where(c => c.ServiceId == serviceId);
        }

        if (!query.Any())
        {
            return NotFound();
        }

        var calendarDtos = await query
            .Select(c => new CalendarDto
            {
                ServiceId = c.ServiceId,
                Monday = c.Monday,
                Tuesday = c.Tuesday,
                Wednesday = c.Wednesday,
                Thursday = c.Thursday,
                Friday = c.Friday,
                Saturday = c.Saturday,
                Sunday = c.Sunday,
                StartDate = c.StartDate,
                EndDate = c.EndDate
            })
            .ToListAsync();

        return Ok(calendarDtos);
    }

    [HttpGet("notes")]
    public async Task<ActionResult<List<NoteDto>>> GetSydneyNotes() =>
        Ok(await _db.Notes.ToListAsync());

    [HttpGet("routes")]
    public async Task<ActionResult<List<DTOs.RouteDto>>> GetSydneyRoutes() => 
        Ok(await _db.Routes.ToListAsync());

    [HttpGet("shapes")]
    public async Task<ActionResult<Dictionary<string, List<ShapeDetails>>>> GetSydneyShapes(string mode) {
        var cacheKey = $"shapes-{mode}";
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, List<ShapeDetails>>? shapes))
        {
            return Ok(shapes);
        }

        shapes = await _db.Shapes
            .Where(s => s.Mode == mode)
            .GroupBy(s => s.Id)
            .ToDictionaryAsync(
                g => g.Key,
                g => g
                    .OrderBy(s => s.Sequence)
                    .Select(s => new ShapeDetails
                    {
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        Sequence = s.Sequence,
                        DistanceTravelled = s.DistanceTravelled
                    })
                    .ToList()
            );

        foreach (var shapeId in shapes.Keys)
        {
            shapes[shapeId] = [.. shapes[shapeId].OrderBy(s => s.DistanceTravelled)];
        }

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

        _cache.Set(cacheKey, shapes, cacheOptions);

        return Ok(shapes);
    }

    [HttpGet("stops")]
    public async Task<ActionResult<List<StopDto>>> GetSydneyStops(string mode) {
        var cacheKey = $"stops-{mode}";
        if (_cache.TryGetValue(cacheKey, out List<StopDto>? stops))
        {
            return Ok(stops);
        }

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
            })
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

        _cache.Set(cacheKey, stops, cacheOptions);

        return Ok(stops);
    }

    [HttpGet("stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyStopTimes(string mode, string stopName, string timeString, bool before)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
        var dayOfWeek = time.DayOfWeek;

        var query = _db.Stops
            .Where(s => s.Mode == mode)
            .Where(s => s.Name.Contains(stopName))
            .Join(_db.StopTimes, s => s.Id, st => st.StopId, (s, st) => new { s, st })
            .Join(_db.Trips, x => x.st.TripId, t => t.Id, (x, t) => new { x.s, x.st, t })
            .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.s, x.st, x.t, c })
            .Where(x => x.c.StartDate <= time && x.c.EndDate >= time)
            .Where(x =>
                (dayOfWeek == DayOfWeek.Monday && x.c.Monday == 1) ||
                (dayOfWeek == DayOfWeek.Tuesday && x.c.Tuesday == 1) ||
                (dayOfWeek == DayOfWeek.Wednesday && x.c.Wednesday == 1) ||
                (dayOfWeek == DayOfWeek.Thursday && x.c.Thursday == 1) ||
                (dayOfWeek == DayOfWeek.Friday && x.c.Friday == 1) ||
                (dayOfWeek == DayOfWeek.Saturday && x.c.Saturday == 1) ||
                (dayOfWeek == DayOfWeek.Sunday && x.c.Sunday == 1)
            )
            .Select(x => new
            {
                x.st.TripId,
                x.st.ArrivalTime,
                x.st.DepartureTime,
                x.t.RouteId,
                x.st.StopSequence,
                x.st.StopHeadSign,
                x.st.PickupType,
                x.st.DropOffType,
                x.st.ShapeDistanceTravelled
            });

        var stopTimesDto = query
            .Select(x => new StopTimeDto
            {
                TripId = x.TripId,
                ArrivalTime = x.ArrivalTime,
                DepartureTime = x.DepartureTime,
                RouteId = x.RouteId,
                StopSequence = x.StopSequence,
                StopHeadSign = x.StopHeadSign,
                PickupType = x.PickupType,
                DropOffType = x.DropOffType,
                ShapeDistanceTravelled = x.ShapeDistanceTravelled,
            })
            .ToList();

        stopTimesDto.Sort((a, b) => a.DepartureTime.CompareTo(b.DepartureTime));
        var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.DepartureTime.Hours >= 4);

        var index = 0;
        for (var i = 0; i < stopTimesDto.Count; i++)
        {
            if (stopTimesDto[i].DepartureTime > time.TimeOfDay)
            {
                index = i;
                break;
            }
        }

        stopTimesDto.Sort((a, b) => 
        {
            bool aIsEarly = a.DepartureTime.Hours < 4;
            bool bIsEarly = b.DepartureTime.Hours < 4;
            
            if (aIsEarly == bIsEarly)
                return a.DepartureTime.CompareTo(b.DepartureTime);

            return aIsEarly ? 1 : -1;
        });

        if (index <= rotatedFirstIndex)
            index = stopTimesDto.Count - (rotatedFirstIndex - index);
        else
            index -= rotatedFirstIndex;

        var futureCount = 24;
        var startIndex = index;
        var endIndex = Math.Min(index + futureCount, stopTimesDto.Count);
        if (before) {
            var priorCount = 12;
            startIndex = Math.Max(0, index - priorCount);
            endIndex = index - 1;
        }

        var slicedStopTimes = stopTimesDto
            .Skip(startIndex)
            .Take(endIndex - startIndex)
            .ToList();

        return Ok(slicedStopTimes);
    }

    private async Task<Dictionary<string, string>> GetSydneyTripHeadsigns(HashSet<string> tripIds)
    {
        var tripHeadsigns = await _db.Trips
            .Where(t => tripIds.Contains(t.Id.ToString()))
            .Select(t => new { t.Id, t.HeadSign })
            .ToListAsync();

        var tripHeadsignsDict = tripHeadsigns.ToDictionary(
            x => x.Id.ToString(),
            x => x.HeadSign
        );

        return tripHeadsignsDict;
    }

    [HttpGet("trips")]
    public async Task<ActionResult<TripDto>> GetSydneyTrips(string tripId)
    {
        var trip = await _db.Trips.Where(t => t.Id == tripId).SingleOrDefaultAsync();

        // invalid tripId
        if (trip == null)
        {
            return NotFound();
        }

        var tripDto = new TripDto
        {
            Id = trip.Id,
            RouteId = trip.RouteId,
            ServiceId = trip.ServiceId,
            ShapeId = trip.ShapeId,
            HeadSign = trip.HeadSign,
            DirectionId = trip.DirectionId,
            ShortName = trip.ShortName ?? string.Empty,
            BlockId = trip.BlockId,
            WheelchairAccessible = trip.WheelchairAccessible,
            TripNote = trip.TripNote,
            RouteDirection = trip.RouteDirection,
            BikesAllowed = trip.BikesAllowed,
        };

        return Ok(tripDto);
    }

    [HttpGet("trip-stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyTripStopTimes(string mode, string tripId, string timeString)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
        var dayOfWeek = time.DayOfWeek;

        var query = _db.StopTimes
            .Where(st => st.Mode == mode)
            .Where(st => st.TripId == tripId)
            .Where(st => st.PickupType == 0 || st.DropOffType == 0)
            .Join(_db.Trips, st => st.TripId, t => t.Id, (st, t) => new { st, t })
            .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.st, x.t, c })
            .Where(x => x.c.StartDate <= time && x.c.EndDate >= time)
            .Where(x =>
                (dayOfWeek == DayOfWeek.Monday && x.c.Monday == 1) ||
                (dayOfWeek == DayOfWeek.Tuesday && x.c.Tuesday == 1) ||
                (dayOfWeek == DayOfWeek.Wednesday && x.c.Wednesday == 1) ||
                (dayOfWeek == DayOfWeek.Thursday && x.c.Thursday == 1) ||
                (dayOfWeek == DayOfWeek.Friday && x.c.Friday == 1) ||
                (dayOfWeek == DayOfWeek.Saturday && x.c.Saturday == 1) ||
                (dayOfWeek == DayOfWeek.Sunday && x.c.Sunday == 1)
            )
            .Join(_db.Stops, x => x.st.StopId, s => s.Id, (x, s) => new { x.st, x.t, x.c, s })
            .Select(x => new
            {
                x.st.TripId,
                x.st.ArrivalTime,
                x.st.DepartureTime,
                x.s.Name,
                x.s.Id,
                x.t.RouteId,
                x.st.StopSequence,
                x.st.StopHeadSign,
                x.st.PickupType,
                x.st.DropOffType,
                x.st.ShapeDistanceTravelled
            });

        var missingHeadsigns = await query
            .Where(x => string.IsNullOrEmpty(x.StopHeadSign))
            .Select(x => x.TripId)
            .ToHashSetAsync();

        var missingHeadsignsDict = await GetSydneyTripHeadsigns(missingHeadsigns) ?? new Dictionary<string, string>();

        var stopTimesDto = await query
            .Select(x => new StopTimeDto
            {
                TripId = x.TripId,
                ArrivalTime = x.ArrivalTime,
                DepartureTime = x.DepartureTime,
                StopName = x.Name,
                StopId = x.Id,
                StopSequence = x.StopSequence,
                StopHeadSign = string.IsNullOrWhiteSpace(x.StopHeadSign) ? missingHeadsignsDict.GetValueOrDefault(x.TripId) : x.StopHeadSign,
                PickupType = x.PickupType,
                DropOffType = x.DropOffType,
                ShapeDistanceTravelled = x.ShapeDistanceTravelled,
            })
            .ToListAsync();

        stopTimesDto.Sort((a, b) => a.DepartureTime.CompareTo(b.DepartureTime));
        var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.DepartureTime.Hours >= 4);

        var index = 0;
        for (var i = 0; i < stopTimesDto.Count; i++)
        {
            if (stopTimesDto[i].DepartureTime > time.TimeOfDay)
            {
                index = i;
                break;
            }
        }

        stopTimesDto.Sort((a, b) =>
        {
            bool aIsEarly = a.DepartureTime.Hours < 4;
            bool bIsEarly = b.DepartureTime.Hours < 4;

            if (aIsEarly == bIsEarly)
                return a.DepartureTime.CompareTo(b.DepartureTime);

            return aIsEarly ? 1 : -1;
        });

        return Ok(stopTimesDto); 
    }

    [HttpGet("realtime-trip-stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyRealtimeTripStopTimes(string mode, string tripId, string timeString)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
        var dayOfWeek = time.DayOfWeek;

        var query = _db.StopTimes
            .Where(st => st.Mode == mode)
            .Where(st => st.TripId == tripId)
            .Where(st => st.PickupType == 0 || st.DropOffType == 0)
            .Join(_db.Trips, st => st.TripId, t => t.Id, (st, t) => new { st, t })
            .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.st, x.t, c })
            .Where(x => x.c.StartDate <= time && x.c.EndDate >= time)
            .Where(x =>
                (dayOfWeek == DayOfWeek.Monday && x.c.Monday == 1) ||
                (dayOfWeek == DayOfWeek.Tuesday && x.c.Tuesday == 1) ||
                (dayOfWeek == DayOfWeek.Wednesday && x.c.Wednesday == 1) ||
                (dayOfWeek == DayOfWeek.Thursday && x.c.Thursday == 1) ||
                (dayOfWeek == DayOfWeek.Friday && x.c.Friday == 1) ||
                (dayOfWeek == DayOfWeek.Saturday && x.c.Saturday == 1) ||
                (dayOfWeek == DayOfWeek.Sunday && x.c.Sunday == 1)
            )
            .Join(_db.Stops, x => x.st.StopId, s => s.Id, (x, s) => new { x.st, x.t, x.c, s })
            .Select(x => new
            {
                x.st.TripId,
                x.st.ArrivalTime,
                x.st.DepartureTime,
                x.s.Name,
                x.s.Id,
                x.t.RouteId,
                x.st.StopSequence,
                x.st.StopHeadSign,
                x.st.PickupType,
                x.st.DropOffType,
                x.st.ShapeDistanceTravelled
            });

        var missingHeadsigns = await query
            .Where(x => string.IsNullOrEmpty(x.StopHeadSign))
            .Select(x => x.TripId)
            .ToHashSetAsync();

        var missingHeadsignsDict = await GetSydneyTripHeadsigns(missingHeadsigns) ?? new Dictionary<string, string>();

        var stopTimesDto = await query
            .Select(x => new StopTimeDto
            {
                TripId = x.TripId,
                ArrivalTime = x.ArrivalTime,
                DepartureTime = x.DepartureTime,
                StopName = x.Name,
                StopId = x.Id,
                StopSequence = x.StopSequence,
                StopHeadSign = string.IsNullOrWhiteSpace(x.StopHeadSign) ? missingHeadsignsDict.GetValueOrDefault(x.TripId) : x.StopHeadSign,
                PickupType = x.PickupType,
                DropOffType = x.DropOffType,
                ShapeDistanceTravelled = x.ShapeDistanceTravelled,
            })
            .ToListAsync();

        var tripUpdate = await _services.SydneyRealtimeTripUpdates(mode, tripId);

        for (var i = 0; i < stopTimesDto.Count; i++)
        {
            var corresp = tripUpdate.StopTimeUpdate.Find((stu) => stu.StopId == stopTimesDto[i].StopId);
            if (corresp == null) continue;

            if (corresp.Arrival != null && corresp.Arrival.Delay != null)
            {
                var scheduledArrivalTime = DateTime.Today;
                scheduledArrivalTime = scheduledArrivalTime.AddHours(stopTimesDto[i].ArrivalTime.Hours);
                scheduledArrivalTime = scheduledArrivalTime.AddMinutes(stopTimesDto[i].ArrivalTime.Minutes);
                scheduledArrivalTime = scheduledArrivalTime.AddSeconds(stopTimesDto[i].ArrivalTime.Seconds);
                scheduledArrivalTime = scheduledArrivalTime.AddSeconds((double)corresp.Arrival.Delay);
                stopTimesDto[i].ArrivalTime = scheduledArrivalTime.TimeOfDay;
                stopTimesDto[i].Delay = corresp.Arrival.Delay;
            }

            if (corresp.Departure != null && corresp.Departure.Delay != null)
            {
                var scheduledDepartureTime = DateTime.Today;
                scheduledDepartureTime = scheduledDepartureTime.AddHours(stopTimesDto[i].DepartureTime.Hours);
                scheduledDepartureTime = scheduledDepartureTime.AddMinutes(stopTimesDto[i].DepartureTime.Minutes);
                scheduledDepartureTime = scheduledDepartureTime.AddSeconds(stopTimesDto[i].DepartureTime.Seconds);
                scheduledDepartureTime = scheduledDepartureTime.AddSeconds((double)corresp.Departure.Delay);
                stopTimesDto[i].DepartureTime = scheduledDepartureTime.TimeOfDay;
                stopTimesDto[i].Delay = corresp.Departure.Delay;
            }
        }

        stopTimesDto.Sort((a, b) => a.DepartureTime.CompareTo(b.DepartureTime));
        var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.DepartureTime.Hours >= 4);

        var index = 0;
        for (var i = 0; i < stopTimesDto.Count; i++)
        {
            if (stopTimesDto[i].DepartureTime > time.TimeOfDay)
            {
                index = i;
                break;
            }
        }

        stopTimesDto.Sort((a, b) =>
        {
            bool aIsEarly = a.DepartureTime.Hours < 4;
            bool bIsEarly = b.DepartureTime.Hours < 4;

            if (aIsEarly == bIsEarly)
                return a.DepartureTime.CompareTo(b.DepartureTime);

            return aIsEarly ? 1 : -1;
        });

        return Ok(stopTimesDto); 
    }

    [HttpGet("realtime-trip-updates")]
    public async Task<ActionResult<TripUpdateDto>> GetSydneyRealtimeUpdates(string mode, string tripId)
    {
        var tripUpdate = await _services.SydneyRealtimeTripUpdates(mode, tripId);

        return Ok(tripUpdate);
    }

    [HttpGet("realtime-vehicle-positions")]
    public async Task<ActionResult<List<VehiclePositionDto>>> GetSydneyRealtimeVehiclePositions(string mode)
    {
        var vehiclePositions = await _services.SydneyRealtimeVehiclePositions(mode);

        return Ok(vehiclePositions);
    }
}
