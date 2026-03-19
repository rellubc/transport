using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;
using TransportApi.DTOs.Realtime;
using TransportApi.Models;
using TransportApi.Services;

using TransitRealtime;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney/trains")]
public class SydneyTrainsController : ControllerBase
{
    private readonly TransportDbContext _db;
    private readonly ISydneyTrainsService _services;

    public SydneyTrainsController(TransportDbContext db, ISydneyTrainsService services)
    {
        _db = db;
        _services = services;
    }

    [HttpGet("agencies")]
    public async Task<ActionResult<List<AgencyDto>>> GetSydneyTrainsAgencies() =>
        Ok(await _db.Agencies.ToListAsync());

    [HttpGet("calendars")]
    public async Task<ActionResult<List<CalendarDto>>> GetSydneyTrainsCalendars(string? serviceId)
    {   
        IQueryable<Calendar> query = _db.Calendars;

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
    public async Task<ActionResult<List<NoteDto>>> GetSydneyTrainsNotes() =>
        Ok(await _db.Notes.ToListAsync());

    [HttpGet("routes")]
    public async Task<ActionResult<List<DTOs.RouteDto>>> GetSydneyTrainsRoutes() => 
        Ok(await _db.Routes.ToListAsync());

    [HttpGet("shapes")]
    public async Task<ActionResult<Dictionary<int, List<ShapeDetails>>>> GetSydneyTrainsShapes() {
        var shapes = await _db.Shapes
            .Where(s => s.Mode == "Rail")
            .ToListAsync();

        Dictionary<string, List<ShapeDetails>> shapesDictionary = [];

        foreach (var shape in shapes)
        {
            if (!shapesDictionary.TryGetValue(shape.Id, out List<ShapeDetails>? value))
            {
                value = [];
                shapesDictionary[shape.Id] = value;
            }

            value.Add(new ShapeDetails
            {
                Latitude = shape.Latitude,
                Longitude = shape.Longitude,
                Sequence = shape.Sequence,
                DistanceTravelled = shape.DistanceTravelled,
            });
        }

        foreach (var shapeId in shapesDictionary.Keys)
        {
            shapesDictionary[shapeId] = [.. shapesDictionary[shapeId].OrderBy(s => s.DistanceTravelled)];
        }

        return Ok(shapesDictionary);
    }

    [HttpGet("stops")]
    public async Task<ActionResult<List<StopDto>>> GetSydneyTrainsStops() {
        var stops = await _db.Stops
            .Where(s => s.Mode == "Rail")
            .ToListAsync();

        var stopsDto = stops
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
        }).ToList();

        return Ok(stopsDto);
    }

    [HttpGet("stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyTrainsStopTimes(string stopName, string timeString, bool before)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
        var dayOfWeek = time.DayOfWeek;

        var query = _db.StopTimes
            .Where(st => st.Mode == "Rail")
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

        stopTimesDto.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));
        var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.ArrivalTime.Hours >= 4);

        var index = 0;
        for (var i = 0; i < stopTimesDto.Count; i++)
        {
            if (stopTimesDto[i].ArrivalTime > time.TimeOfDay)
            {
                index = i;
                break;
            }
        }

        stopTimesDto.Sort((a, b) => 
        {
            bool aIsEarly = a.ArrivalTime.Hours < 4;
            bool bIsEarly = b.ArrivalTime.Hours < 4;
            
            if (aIsEarly == bIsEarly)
                return a.ArrivalTime.CompareTo(b.ArrivalTime);

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

    private async Task<Dictionary<string, string>> GetSydneyTrainsTripHeadsigns(HashSet<string> tripIds)
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
    public async Task<ActionResult<TripDto>> GetSydneyTrainsTrips(string tripId)
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
    public async Task<ActionResult<StopTimeDto>> GetSydneyTrainsTripStopTimes(string tripId, string timeString)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
        var dayOfWeek = time.DayOfWeek;

        var query = _db.StopTimes
            .Where(st => st.Mode == "Rail")
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

        var stopTimesDto = await query
            .Select(x => new StopTimeDto
            {
                TripId = x.TripId,
                ArrivalTime = x.ArrivalTime,
                DepartureTime = x.DepartureTime,
                StopName = x.Name,
                StopId = x.Id,
                StopSequence = x.StopSequence,
                StopHeadSign = x.StopHeadSign,
                PickupType = x.PickupType,
                DropOffType = x.DropOffType,
                ShapeDistanceTravelled = x.ShapeDistanceTravelled,
            })
            .ToListAsync();

        stopTimesDto.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));
        var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.ArrivalTime.Hours >= 4);

        var index = 0;
        for (var i = 0; i < stopTimesDto.Count; i++)
        {
            if (stopTimesDto[i].ArrivalTime > time.TimeOfDay)
            {
                index = i;
                break;
            }
        }

        stopTimesDto.Sort((a, b) =>
        {
            bool aIsEarly = a.ArrivalTime.Hours < 4;
            bool bIsEarly = b.ArrivalTime.Hours < 4;

            if (aIsEarly == bIsEarly)
                return a.ArrivalTime.CompareTo(b.ArrivalTime);

            return aIsEarly ? 1 : -1;
        });

        return Ok(stopTimesDto); 
    }

    [HttpGet("realtime-trip-updates")]
    public async Task<ActionResult<TripUpdateDto>> GetSydneyTrainsRealtimeStopTimes(string tripId)
    {
        var tripUpdate = await _services.SydneyTrainsRealtimeTripUpdates(tripId);

        return Ok(tripUpdate);
    }

    [HttpGet("realtime-vehicle-positions")]
    public async Task<ActionResult<List<VehiclePositionDto>>> GetSydneyTrainsRealtimeVehiclePositions()
    {
        var vehiclePositions = await _services.SydneyTrainsRealtimeVehiclePositions();

        return Ok(vehiclePositions);
    }
}
