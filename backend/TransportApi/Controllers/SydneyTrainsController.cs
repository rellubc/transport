using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;
using TransportApi.DTOs.Realtime;
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
    public async Task<ActionResult<List<CalendarDto>>> GetSydneyTrainsCalendars(string serviceId)
    {
        var calendar = await _db.Calendars.Where(c => c.ServiceId == serviceId).SingleOrDefaultAsync();

        // invalid serviceId
        if (calendar == null)
        {
            return NotFound();
        }

        var calendarDto = new CalendarDto
        {
            ServiceId = calendar.ServiceId,
            Monday = calendar.Monday,
            Tuesday = calendar.Tuesday,
            Wednesday = calendar.Wednesday,
            Thursday = calendar.Thursday,
            Friday = calendar.Friday,
            Saturday = calendar.Saturday,
            Sunday = calendar.Sunday,
            StartDate = calendar.StartDate,
            EndDate = calendar.EndDate
        };

        return Ok(calendarDto);
    }

    [HttpGet("calendar-dates")]
    public async Task<ActionResult<List<CalendarDateDto>>> GetSydneyTrainsCalendarDates() =>
        Ok(await _db.CalendarDates.ToListAsync());

    [HttpGet("notes")]
    public async Task<ActionResult<List<NoteDto>>> GetSydneyTrainsNotes() =>
        Ok(await _db.Notes.ToListAsync());

    [HttpGet("routes")]
    public async Task<ActionResult<List<DTOs.RouteDto>>> GetSydneyTrainsRoutes() => 
        Ok(await _db.Routes.ToListAsync());

    [HttpGet("shapes")]
    public async Task<ActionResult<Dictionary<int, List<ShapeDetails>>>> GetSydneyTrainsShapes() {
        var shapes = await _db.Shapes.ToListAsync();

        Dictionary<string, List<ShapeDetails>> shapesDictionary = [];

        foreach (var shape in shapes)
        {
            if (shape.Mode != "sydneytrains") continue;
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
                Mode = shape.Mode,
            });
        }

        foreach (var shapeId in shapesDictionary.Keys)
        {
            shapesDictionary[shapeId] = [.. shapesDictionary[shapeId].OrderBy(s => s.Sequence)];
        }

        return Ok(shapesDictionary);
    }

    [HttpGet("stops")]
    public async Task<ActionResult<List<StopDto>>> GetSydneyTrainsStops() {
        var stops = await _db.Stops.ToListAsync();

        var stopsDto = stops
        .Where(s => s.Mode == "sydneytrains")
        .Select(s => new StopDto
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name,
            Description = s.Description,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            ZoneId = s.ZoneId,
            Url = s.Url,
            LocationType = s.LocationType,
            ParentStationId = s.ParentStationId,
            Timezone = s.Timezone,
            WheelchairBoarding = s.WheelchairBoarding,
            Mode = s.Mode,
        }).ToList();

        return Ok(stopsDto);
    }

    [HttpGet("stops-platforms")]
    public async Task<ActionResult<List<StopDto>>> GetSydneyTrainsStopsPlatforms(string stopId) {
        var stops = await _db.Stops.ToListAsync();

        var stopsDto = stops
        .Where(s => s.Mode == "sydneytrains" && s.ParentStationId == stopId)
        .Select(s => new StopDto
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name,
            Description = s.Description,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            ZoneId = s.ZoneId,
            Url = s.Url,
            LocationType = s.LocationType,
            ParentStationId = s.ParentStationId,
            Timezone = s.Timezone,
            WheelchairBoarding = s.WheelchairBoarding,
            Mode = s.Mode,
        }).ToList();

        return Ok(stopsDto);
    }

    [HttpGet("stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyTrainsStopTimes(string stopId)
    {
        var today = DateTime.UtcNow.Date;
        var dayOfWeek = today.DayOfWeek;

        var query = _db.StopTimes
            .Where(st => st.StopId == stopId)
            .Join(
                _db.Trips,
                st => st.TripId,
                t => t.Id,
                (st, t) => new { st, t }
            )
            .Join(
                _db.Calendars,
                x => x.t.ServiceId,
                c => c.ServiceId,
                (x, c) => new { x.st, x.t, c }
            )
            .Where(x => x.c.StartDate <= today && x.c.EndDate >= today);

        query = dayOfWeek switch
        {
            DayOfWeek.Monday    => query.Where(x => x.c.Monday),
            DayOfWeek.Tuesday   => query.Where(x => x.c.Tuesday),
            DayOfWeek.Wednesday => query.Where(x => x.c.Wednesday),
            DayOfWeek.Thursday  => query.Where(x => x.c.Thursday),
            DayOfWeek.Friday    => query.Where(x => x.c.Friday),
            DayOfWeek.Saturday  => query.Where(x => x.c.Saturday),
            DayOfWeek.Sunday    => query.Where(x => x.c.Sunday),
            _ => query
        };

        var stopTimesDto = query
            .Join(
                _db.Stops,
                x => x.st.StopId,
                s => s.Id,
                (x, s) => new StopTimeDto
                {
                    TripId = x.st.TripId,
                    ArrivalTime = x.st.ArrivalTime,
                    DepartureTime = x.st.DepartureTime,
                    StopId = x.st.StopId,
                    StopName = s.Name,
                    RouteId = x.t.RouteId,
                    StopSequence = x.st.StopSequence,
                    StopHeadSign = x.st.StopHeadSign,
                    PickupType = x.st.PickupType,
                    DropOffType = x.st.DropOffType,
                    ShapeDistanceTravelled = x.st.ShapeDistanceTravelled,
                    Mode = x.st.Mode
                })
            .ToList();

        return Ok(stopTimesDto);
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
            VehicleCategoryId = trip.VehicleCategoryId,
        };

        return Ok(tripDto);
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
