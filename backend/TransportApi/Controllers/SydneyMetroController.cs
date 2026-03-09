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
[Route("api/sydney/metro")]
public class SydneyMetroController : ControllerBase
{
    private readonly TransportDbContext _db;
    private readonly ISydneyMetroService _services;

    public SydneyMetroController(TransportDbContext db, ISydneyMetroService services)
    {
        _db = db;
        _services = services;
    }

    [HttpGet("agencies")]
    public async Task<ActionResult<List<AgencyDto>>> GetSydneyMetroAgencies() =>
        Ok(await _db.Agencies.ToListAsync());

    [HttpGet("calendars")]
    public async Task<ActionResult<List<CalendarDto>>> GetSydneyMetroCalendars(string serviceId)
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
    public async Task<ActionResult<List<CalendarDateDto>>> GetSydneyMetroCalendarDates() =>
        Ok(await _db.CalendarDates.ToListAsync());

    [HttpGet("notes")]
    public async Task<ActionResult<List<NoteDto>>> GetSydneyMetroNotes() =>
        Ok(await _db.Notes.ToListAsync());

    [HttpGet("routes")]
    public async Task<ActionResult<List<DTOs.RouteDto>>> GetSydneyMetroRoutes() => 
        Ok(await _db.Routes.ToListAsync());

    [HttpGet("shapes")]
    public async Task<ActionResult<Dictionary<int, List<ShapeDetails>>>> GetSydneyMetroShapes() {
        var shapes = await _db.Shapes.ToListAsync();

        Dictionary<string, List<ShapeDetails>> shapesDictionary = [];

        foreach (var shape in shapes)
        {
            if (shape.Mode != "metro") continue;
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
            shapesDictionary[shapeId] = [.. shapesDictionary[shapeId].OrderBy(s => s.DistanceTravelled)];
        }

        return Ok(shapesDictionary);
    }

    [HttpGet("stops")]
    public async Task<ActionResult<List<StopDto>>> GetSydneyMetroStops() {
        var stops = await _db.Stops.ToListAsync();

        var stopsDto = stops
        .Where(s => s.Mode == "metro")
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
        }).ToList();

        return Ok(stopsDto);
    }

    [HttpGet("stop-times")]
    public async Task<ActionResult<List<StopTime>>> GetSydneyMetroStopTimes(string stopId)
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
                    Timepoint = x.st.Timepoint,
                    StopNote = x.st.StopNote,
                    Mode = x.st.Mode
                })
            .ToList();

        return Ok(stopTimesDto);
    }

    [HttpGet("trips")]
    public async Task<ActionResult<TripDto>> GetSydneyMetroTrips(string tripId)
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
            ShortName = trip.ShortName,
            BlockId = trip.BlockId,
            WheelchairAccessible = trip.WheelchairAccessible,
            TripNote = trip.TripNote,
            RouteDirection = trip.RouteDirection,
            BikesAllowed = trip.BikesAllowed,
        };

        return Ok(tripDto);
    }

    [HttpGet("realtime-trip-updates")]
    public async Task<ActionResult<TripUpdateDto>> GetSydneyMetroRealtimeStopTimes(string tripId)
    {
        var tripUpdate = await _services.MetroRealtimeTripUpdates(tripId);

        return Ok(tripUpdate);
    }

    [HttpGet("realtime-vehicle-positions")]
    public async Task<ActionResult<List<VehiclePositionDto>>> GetSydneyMetroRealtimeVehiclePositions()
    {
        var vehiclePositions = await _services.MetroRealtimeVehiclePositions();

        return Ok(vehiclePositions);
    }
}
