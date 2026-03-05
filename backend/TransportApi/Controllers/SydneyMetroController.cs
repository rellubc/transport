using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;
using TransportApi.DTOs.Realtime;
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
    public async Task<ActionResult<List<CalendarDto>>> GetSydneyMetroCalendars()
    {
        var calendars = await _db.Calendars.ToListAsync();

        var calendarsDto = calendars
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
        }).ToList();

        return Ok(calendarsDto);
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
            shapesDictionary[shapeId] = [.. shapesDictionary[shapeId].OrderBy(s => s.DistanceTravelled)];
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
            LocationType = s.LocationType,
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
            LocationType = s.LocationType,
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
    public async Task<ActionResult<TripDto>> GetSydneyMetroTrips(string tripId)
    {
        var trip = await _db.Trips.Where(t => t.Id == tripId).SingleOrDefaultAsync();

        Console.WriteLine(tripId);
        Console.WriteLine(trip);

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

    [HttpGet("realtime-stop-times")]
    public async Task<ActionResult<RealtimeTripUpdateDto>> GetSydneyMetroRealtimeStopTimes(string tripId)
    {
        var tripUpdate = await _services.SydneyMetroTripUpdates(tripId);
        var tripUpdateDto = new RealtimeTripUpdateDto();

        TimeZoneInfo tz;
        DateTime utc;
        DateTime sydneyTime;

        var newTripDescriptor = new TripDescriptorDto
        {
            TripId = tripUpdate.Trip.TripId,
            RouteId = tripUpdate.Trip.RouteId,
            DirectionId = tripUpdate.Trip.DirectionId,
            StartTime = tripUpdate.Trip.StartTime,
            StartDate = tripUpdate.Trip.StartDate,
            ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipTripDescriptor(tripUpdate.Trip.ScheduleRelationship)
        };

        var newVehicleDescriptor = new VehicleDescriptorDto
        {
            Id = tripUpdate.Vehicle.Id,
            Label = tripUpdate.Vehicle.Label,
            LicensePlate = tripUpdate.Vehicle.LicensePlate,
        };

        var newStopTimeUpdates = new List<StopTimeUpdateDto>();
        foreach(var stopTimeUpdate in tripUpdate.StopTimeUpdate)
        {
            var newArrival = new StopTimeEventDto();
            if (stopTimeUpdate.Arrival != null) {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
                utc = DateTimeOffset.FromUnixTimeSeconds(stopTimeUpdate.Arrival.Time).UtcDateTime;
                sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
                newArrival = new StopTimeEventDto
                {
                    Delay = stopTimeUpdate.Arrival.Delay,
                    Time = sydneyTime,
                    Uncertainty = stopTimeUpdate.Arrival.Uncertainty
                };
            }

            var newDeparture = new StopTimeEventDto();
            if (stopTimeUpdate.Departure != null) {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
                utc = DateTimeOffset.FromUnixTimeSeconds(stopTimeUpdate.Departure.Time).UtcDateTime;
                sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
                newDeparture = new StopTimeEventDto
                {
                    Delay = stopTimeUpdate.Departure.Delay,
                    Time = sydneyTime,
                    Uncertainty = stopTimeUpdate.Departure.Uncertainty
                };
            }

            var stopName = await _db.Stops.Where(s => s.Id == int.Parse(stopTimeUpdate.StopId)).Select(s => s.Name).FirstOrDefaultAsync();

            var newStopTimeUpdate = new StopTimeUpdateDto
            {
                StopSequence = stopTimeUpdate.StopSequence,
                StopId = stopTimeUpdate.StopId,
                StopName = stopName,
                Arrival = newArrival,
                Departure = newDeparture,
                ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipStopTimeUpdate(stopTimeUpdate.ScheduleRelationship),
                DepartureOccupancyStatus = RealtimeEnums.MapOccupancyStatusStopTimeUpdate(stopTimeUpdate.DepartureOccupancyStatus),
            };

            newStopTimeUpdates.Add(newStopTimeUpdate);
        }

        tz = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
        utc = DateTimeOffset.FromUnixTimeSeconds((long)tripUpdate.Timestamp).UtcDateTime;
        sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);

        var newTripUpdateDto = new RealtimeTripUpdateDto
        {
            Trip = newTripDescriptor,
            Vehicle = newVehicleDescriptor,
            StopTimeUpdate = newStopTimeUpdates,
            Timestamp = sydneyTime,
            Delay = tripUpdate.Delay
        };

        tripUpdateDto = newTripUpdateDto;

        return Ok(tripUpdateDto);
    }

    [HttpGet("realtime-vehicle-positions")]
    public async Task<ActionResult<List<RealtimeVehiclePositionDto>>> GetSydneyMetroRealtimeVehiclePositions()
    {
        var vehiclePositions = await _services.SydneyMetroVehiclePositions();
        var vehiclePositionsDto = new List<RealtimeVehiclePositionDto>();
        
        foreach (var tripUpdate in vehiclePositions)
        {
            if (tripUpdate.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor) != null)
            {
                Console.WriteLine("yes");
            }

            var newVehicleDescriptor = new VehicleDescriptorDto
            {
                Id = tripUpdate.Vehicle.Id,
                Label = tripUpdate.Vehicle.Label,
                LicensePlate = tripUpdate.Vehicle.LicensePlate,
            };

            var newPosition = new PositionDto
            {
                Latitude = tripUpdate.Position.Latitude,
                Longitude = tripUpdate.Position.Longitude,
                Bearing = tripUpdate.Position.Bearing,
                Odometer = tripUpdate.Position.Odometer,
                Speed = tripUpdate.Position.Speed
            };

            var newTripDescriptor = new TripDescriptorDto
            {
                TripId = tripUpdate.Trip.TripId,
                RouteId = tripUpdate.Trip.RouteId,
                DirectionId = tripUpdate.Trip.DirectionId,
                StartTime = tripUpdate.Trip.StartTime,
                StartDate = tripUpdate.Trip.StartDate,
                ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipTripDescriptor(tripUpdate.Trip.ScheduleRelationship)
            };

            var tz = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
            var utc = DateTimeOffset.FromUnixTimeSeconds((long)tripUpdate.Timestamp).UtcDateTime;
            var sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);

            var newVehiclePosition = new RealtimeVehiclePositionDto
            {
                Vehicle = newVehicleDescriptor,
                Position = newPosition,
                Trip = newTripDescriptor,
                CurrentStopSequence = tripUpdate.CurrentStopSequence,
                StopId = tripUpdate.StopId,
                CurrentStatus = RealtimeEnums.MapVehicleStopStatus(tripUpdate.CurrentStatus),
                Timestamp = sydneyTime,
                CongestionLevel = RealtimeEnums.MapCongestionLevel(tripUpdate.CongestionLevel),
                OccupancyStatus = RealtimeEnums.MapOccupancyStatusVehiclePosition(tripUpdate.OccupancyStatus)
            };

            vehiclePositionsDto.Add(newVehiclePosition);
        }
        return Ok(vehiclePositionsDto);
    }
}
