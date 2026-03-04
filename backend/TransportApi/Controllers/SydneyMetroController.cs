using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;
using TransportApi.DTOs.Realtime;
using TransportApi.Services;
using TransportApi.Utils;

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
    public async Task<ActionResult<List<RealtimeTripUpdateDto>>> GetSydneyMetroRealtimeStopTimes(string tripId)
    {
        var tripUpdates = await _services.SydneyMetroTripUpdates(tripId);
        var tripUpdatesDto = new List<RealtimeTripUpdateDto>();
        
        foreach (var tripUpdate in tripUpdates)
        {
            if (tripUpdate.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor) != null)
            {
                Console.WriteLine("asdf");
            }

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
                    newArrival = new StopTimeEventDto
                    {
                        Delay = stopTimeUpdate.Arrival.Delay,
                        Time = stopTimeUpdate.Arrival.Time,
                        Uncertainty = stopTimeUpdate.Arrival.Uncertainty
                    };
                }

                var newDeparture = new StopTimeEventDto();
                if (stopTimeUpdate.Departure != null) {
                    newDeparture = new StopTimeEventDto
                    {
                        Delay = stopTimeUpdate.Departure.Delay,
                        Time = stopTimeUpdate.Departure.Time,
                        Uncertainty = stopTimeUpdate.Departure.Uncertainty
                    };
                }

                var newStopTimeUpdate = new StopTimeUpdateDto
                {
                    StopSequence = stopTimeUpdate.StopSequence,
                    StopId = stopTimeUpdate.StopId,
                    Arrival = newArrival,
                    Departure = newDeparture,
                    ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipStopTimeUpdate(stopTimeUpdate.ScheduleRelationship),
                    DepartureOccupancyStatus = RealtimeEnums.MapOccupancyStatusStopTimeUpdate(stopTimeUpdate.DepartureOccupancyStatus),
                };

                newStopTimeUpdates.Add(newStopTimeUpdate);
            }

            var newTripUpdateDto = new RealtimeTripUpdateDto
            {
                Trip = newTripDescriptor,
                Vehicle = newVehicleDescriptor,
                StopTimeUpdate = newStopTimeUpdates,
                Timestamp = tripUpdate.Timestamp,
                Delay = tripUpdate.Delay
            };

            tripUpdatesDto.Add(newTripUpdateDto);
        }

        return Ok(tripUpdatesDto);
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

            var newVehiclePosition = new RealtimeVehiclePositionDto
            {
                Vehicle = newVehicleDescriptor,
                Position = newPosition,
                Trip = newTripDescriptor,
                CurrentStopSequence = tripUpdate.CurrentStopSequence,
                StopId = tripUpdate.StopId,
                CurrentStatus = RealtimeEnums.MapVehicleStopStatus(tripUpdate.CurrentStatus),
                Timestamp = tripUpdate.Timestamp,
                CongestionLevel = RealtimeEnums.MapCongestionLevel(tripUpdate.CongestionLevel),
                OccupancyStatus = RealtimeEnums.MapOccupancyStatusVehiclePosition(tripUpdate.OccupancyStatus)
            };

            vehiclePositionsDto.Add(newVehiclePosition);
        }
        return Ok(vehiclePositionsDto);
        // return Ok(await _services.SydneyMetroVehiclePositions());
    }
}
