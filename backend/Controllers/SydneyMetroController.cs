using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/sydney/metro")]
public class SydneyMetroController : ControllerBase
{
    private readonly TransportDbContext _db;

    public SydneyMetroController(TransportDbContext db)
    {
        _db = db;
    }

    [HttpGet("agencies")]
    public async Task<ActionResult<List<AgencyDto>>> GetSydneyMetroAgencies() =>
        Ok(await _db.Agencies.ToListAsync());

    [HttpGet("calendars")]
    public async Task<ActionResult<List<CalendarDto>>> GetSydneyMetroCalendars() =>
        Ok(await _db.Calendars.ToListAsync());

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
    public async Task<ActionResult<List<ShapeDto>>> GetSydneyMetroShapes() =>
        Ok(await _db.Shapes.ToListAsync());

    [HttpGet("stops")]
    public async Task<ActionResult<List<StopDto>>> GetSydneyMetroStops() {
        var stops = await _db.Stops.ToListAsync();

        Console.WriteLine($"Found {stops.Count} stops");

        var stopsDto = stops.Select(s => new StopDto
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

        Console.WriteLine($"Returning {stopsDto.Count} stops");

        return Ok(stopsDto);
    }

    [HttpGet("stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyMetroStopTimes() =>
        Ok(await _db.StopTimes.ToListAsync());

    [HttpGet("trips")]
    public async Task<ActionResult<List<TripDto>>> GetSydneyMetroTrips() =>
        Ok(await _db.Trips.ToListAsync());
}
