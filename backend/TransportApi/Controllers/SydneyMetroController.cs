using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;
using TransportApi.Services;
using TransportApi.Utils;

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
    public async Task<ActionResult<Dictionary<int, List<ShapeDetails>>>> GetSydneyMetroShapes() {
        var shapes = await _db.Shapes.ToListAsync();

        Dictionary<int, List<ShapeDetails>> shapesDictionary = [];

        foreach (var shape in shapes)
        {
            if (!shapesDictionary.ContainsKey(shape.Id))
            {
                shapesDictionary[shape.Id] = [];
            }
            shapesDictionary[shape.Id].Add(new ShapeDetails
            {
                Latitude = shape.Latitude,
                Longitude = shape.Longitude,
                Sequence = shape.Sequence,
                DistanceTravelled = shape.DistanceTravelled
            });
        }

        foreach (var shapeId in shapesDictionary.Keys)
        {
            shapesDictionary[shapeId] = [.. shapesDictionary[shapeId].OrderBy(s => s.Sequence)];
        }

        return Ok(shapesDictionary);
    }

    [HttpGet("stops")]
    public async Task<ActionResult<List<StopDto>>> GetSydneyMetroStops() {
        var stops = await _db.Stops.ToListAsync();

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

    [HttpGet("stations")]
    public async Task<ActionResult<List<StopStationDto>>> GetSydneyMetroStations() {
        var stops = await _db.Stops.ToListAsync();

        var stopsDto = stops
        .Where(s => s.LocationType == "Station")
        .Select(s => new StopStationDto
        {
            Id = s.Id,
            Name = s.Name,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
        }).ToList();

        return Ok(stopsDto);
    }

    [HttpGet("platforms")]
    public async Task<ActionResult<List<StopPlatformDto>>> GetSydneyMetroPlatforms() {
        var stops = await _db.Stops.ToListAsync();

        var stopsDto = stops
        .Where(s => s.LocationType == "Platform")
        .Select(s => new StopPlatformDto
        {
            Id = s.Id,
            Name = s.Name,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            ParentStationId = s.ParentStationId,
            WheelchairBoarding = s.WheelchairBoarding,
            PlatformCode = s.PlatformCode,
        }).ToList();

        return Ok(stopsDto);
    }

    [HttpGet("stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyMetroStopTimes(int stopId)
    {
        var stopTimes = await _db.StopTimes.Where(st => st.StopId == stopId).ToListAsync();

        var stopTimesDto = stopTimes
        .Where(st => st.StopId == stopId)
        .Select(st => new StopTimeDto
        {
            TripId = st.TripId,
            ArrivalTime = st.ArrivalTime,
            DepartureTime = st.DepartureTime,
            StopId = st.StopId,
            StopSequence = st.StopSequence,
            StopHeadSign = st.StopHeadSign,
            PickupType = st.PickupType,
            DropOffType = st.DropOffType,
            ShapeDistanceTravelled = st.ShapeDistanceTravelled,
            Timepoint = st.Timepoint,
            StopNote = st.StopNote,
        }).ToList();

        return Ok(stopTimesDto);
    }

    [HttpGet("trips")]
    public async Task<ActionResult<List<TripDto>>> GetSydneyMetroTrips() =>
        Ok(await _db.Trips.ToListAsync());

    [HttpGet("realtime-stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyMetroRealTimeStopTimes(int stopId)
    {
        var realtimeStopTimeUpdates = await _db.RealtimeStopTimeUpdates.ToListAsync();

        var realtimeStopTimeUpdatesDto = realtimeStopTimeUpdates
        .Select(rtsu => new RealtimeStopTimeUpdateDto
        {
            EntityId = rtsu.EntityId,
            TripId = rtsu.TripId,
            StopSequence = rtsu.StopSequence,
            StopId = rtsu.StopId,
            ArrivalTime = rtsu.ArrivalTime,
            DepartureTime = rtsu.DepartureTime,
            ScheduleRelationship = rtsu.ScheduleRelationship,
            InsertedAt = rtsu.InsertedAt,
        }).ToList();

        return Ok(realtimeStopTimeUpdatesDto);
    }

    [HttpGet("realtime-vehicle-positions")]
    public async Task<ActionResult<List<RealtimeVehiclePositionDto>>> GetSydneyMetroRealTimeVehiclePositions()
    {
        await _services.RealtimeSydneyMetroVehiclePos();
        var realtimeVehiclePositions = await _db.RealtimeVehiclePositions.ToListAsync();

        var realtimeVehiclePositionsDto = realtimeVehiclePositions
        .Select(rvp => new RealtimeVehiclePositionDto
        {
            EntityId = rvp.EntityId,
            VehicleId = rvp.VehicleId,
            Label = rvp.Label,
            LicensePlate = rvp.LicensePlate,
            Latitude = rvp.Latitude,
            Longitude = rvp.Longitude,
            Bearing = rvp.Bearing,
            Speed = rvp.Speed,
            TripId = rvp.TripId,
            CurrentStopSequence = rvp.CurrentStopSequence,
            StopId = rvp.StopId,
            CurrentStatus = rvp.CurrentStatus,
            Timestamp = rvp.Timestamp,
            CongestionLevel = rvp.CongestionLevel,
            OccupancyStatus = rvp.OccupancyStatus,
            InsertedAt = rvp.InsertedAt,
        }).ToList();

        return Ok(realtimeVehiclePositionsDto);
    }
}
