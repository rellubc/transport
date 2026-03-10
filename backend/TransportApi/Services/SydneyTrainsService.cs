using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.Models;
using TransportApi.DTOs.Realtime;

using Google.Protobuf;
using System.Xml.Serialization;
using System.Text.Json;

using TransitRealtime;
using TransportApi.DTOs;

namespace TransportApi.Services;

public interface ISydneyTrainsService
{
    Task<TripUpdateDto> SydneyTrainsRealtimeTripUpdates(string TripId);
    Task<List<VehiclePositionDto>> SydneyTrainsRealtimeVehiclePositions();
}

public class SydneyTrainsService(TransportDbContext db, IHttpClientFactory factory, ILogger<SydneyTrainsService> logger) : ISydneyTrainsService
{
    private readonly IHttpClientFactory _factory = factory;
    private readonly TransportDbContext _db = db;
    private readonly ILogger<SydneyTrainsService> _logger = logger;

    public async Task<TripUpdateDto> SydneyTrainsRealtimeTripUpdates(string tripId)
    {
        _logger.LogInformation("Updating vehicle trip details...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v2/gtfs/realtime/sydneytrains");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return new TripUpdateDto();
        }

        ExtensionRegistry registry = [GtfsRealtime1007ExtensionExtensions.Update, GtfsRealtime1007ExtensionExtensions.Consist, GtfsRealtime1007ExtensionExtensions.TrackDirection, GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor, GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy];
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var parser = FeedMessage.Parser.WithExtensionRegistry(registry);
        var feed = parser.ParseFrom(responseStream);

        var newTripUpdate = new TripUpdateDto();

        if (feed == null) return new TripUpdateDto();

        foreach (var entity in feed.Entity)
        {
            try
            {
                if (entity.TripUpdate == null) continue;
                if (entity.TripUpdate.Trip.TripId != tripId) continue;
                newTripUpdate = await TransformTripUpdate(entity.TripUpdate);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process realtime entity {EntityId}", entity.Id);
            }
        }

        return newTripUpdate;
    }

    public async Task<List<VehiclePositionDto>> SydneyTrainsRealtimeVehiclePositions()
    {
        _logger.LogInformation("Updating vehicle positions...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v2/gtfs/vehiclepos/sydneytrains");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return [];
        }

        ExtensionRegistry registry = [GtfsRealtime1007ExtensionExtensions.Update, GtfsRealtime1007ExtensionExtensions.Consist, GtfsRealtime1007ExtensionExtensions.TrackDirection, GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor, GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy];
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var parser = FeedMessage.Parser.WithExtensionRegistry(registry);
        var feed = parser.ParseFrom(responseStream);

        var newVehiclePositions = new List<VehiclePositionDto>();

        if (feed == null) return [];

        foreach (var entity in feed.Entity)
        {
            try
            {
                if (entity.Vehicle == null) continue;
                newVehiclePositions.Add(TransformVehiclePosition(entity.Vehicle));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process vehicle entity {EntityId}", entity.Id);
            }
        }
        return newVehiclePositions;
    }

    private async Task<TripUpdateDto> TransformTripUpdate(TripUpdate tripUpdate)
    {
        var stopIds = tripUpdate.StopTimeUpdate
            .Select(stu => stu.StopId)
            .Distinct()
            .ToList();

        var stopNames = await _db.Stops
            .Where(s => stopIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        var trip = new TripDescriptorDto
        {
            TripId = tripUpdate.Trip.TripId,
            RouteId = tripUpdate.Trip.RouteId,
            DirectionId = tripUpdate.Trip.DirectionId,
            StartTime = tripUpdate.Trip.StartTime,
            StartDate = tripUpdate.Trip.StartDate,
            ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipTripDescriptor(tripUpdate.Trip.ScheduleRelationship) 
        };

        var tfnswVehicleDescriptor = new TfnswVehicleDescriptorDto();
        if (tripUpdate.Vehicle.HasExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
        {
            TfnswVehicleDescriptor tfnswVehicleDescriptorExtension = tripUpdate.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
            tfnswVehicleDescriptor.AirConditioned = tfnswVehicleDescriptorExtension.AirConditioned;
            tfnswVehicleDescriptor.WheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
            tfnswVehicleDescriptor.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
            tfnswVehicleDescriptor.PerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip;
            tfnswVehicleDescriptor.SpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
        }

        var vehicle = new VehicleDescriptorDto
        {
            Id = tripUpdate.Vehicle.Id,
            Label = tripUpdate.Vehicle.Label,
            LicensePlate = tripUpdate.Vehicle.LicensePlate,
            TfnswVehicleDescriptor = tfnswVehicleDescriptor,
        };

        var newStopTimeUpdates = new List<StopTimeUpdateDto>();
        foreach (var stu in tripUpdate.StopTimeUpdate)
        {
            var arrival = new StopTimeEventDto();
            if (stu.Arrival != null) {
                arrival.Delay = stu.Arrival.Delay;
                arrival.Time = stu.Arrival.Time;
                arrival.Uncertainty = stu.Arrival.Uncertainty;
            }

            var departure = new StopTimeEventDto();
            if (stu.Departure != null) {
                departure.Delay = stu.Departure.Delay;
                departure.Time = stu.Departure.Time;
                departure.Uncertainty = stu.Departure.Uncertainty;
            }

            var carriageSeqPredictiveOccupancy = new List<CarriageDescriptorDto>();
            var CarriageSeqPredictiveOccupancyExtension = stu.GetExtension(GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy);
            if (CarriageSeqPredictiveOccupancyExtension != null)
            {
                foreach (var cspo in CarriageSeqPredictiveOccupancyExtension)
                {
                    carriageSeqPredictiveOccupancy.Add(new CarriageDescriptorDto
                    {
                        Name = cspo.Name,
                        PositionInConsist = cspo.PositionInConsist,
                        OccupancyStatus = RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cspo.OccupancyStatus),
                        QuietCarriage = cspo.QuietCarriage,
                        Toilet = RealtimeEnums.MapToiletStatus(cspo.Toilet),
                        LuggageRack = cspo.LuggageRack,
                        DepartureOccupancyStatus = RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cspo.DepartureOccupancyStatus)
                    });
                }
            }

            newStopTimeUpdates.Add(new StopTimeUpdateDto
            {
                StopSequence = stu.StopSequence,
                StopName = stopNames.GetValueOrDefault(stu.StopId ?? "") ?? "",
                StopId = stu.StopId ?? "",
                Arrival = arrival,
                Departure = departure,
                ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipStopTimeUpdate(stu.ScheduleRelationship),
                DepartureOccupancyStatus = RealtimeEnums.MapOccupancyStatusStopTimeUpdate(stu.DepartureOccupancyStatus),
                CarriageSeqPredictiveOccupancy = carriageSeqPredictiveOccupancy,
            });
        }

        var timestamp = tripUpdate.Timestamp;
        var delay = tripUpdate.Delay;

        return new TripUpdateDto
        {
            Trip = trip,
            Vehicle = vehicle,
            StopTimeUpdate = newStopTimeUpdates,
            Timestamp = timestamp,
            Delay = delay
        };
    }

    private static VehiclePositionDto TransformVehiclePosition(VehiclePosition vehiclePosition)
    {
        var trip = new TripDescriptorDto();
        if (vehiclePosition.Trip != null)
        {
            trip.TripId = vehiclePosition.Trip.TripId;
            trip.RouteId = vehiclePosition.Trip.RouteId;
            trip.DirectionId = vehiclePosition.Trip.DirectionId;
            trip.StartTime = vehiclePosition.Trip.StartTime;
            trip.StartDate = vehiclePosition.Trip.StartDate;
            trip.ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipTripDescriptor(vehiclePosition.Trip.ScheduleRelationship);
        }

        var tfnswVehicleDescriptor = new TfnswVehicleDescriptorDto();
        if (vehiclePosition.Vehicle.HasExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
        {
            var tfnswVehicleDescriptorExtension = vehiclePosition.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
            tfnswVehicleDescriptor.AirConditioned = tfnswVehicleDescriptorExtension.AirConditioned;
            tfnswVehicleDescriptor.WheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
            tfnswVehicleDescriptor.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
            tfnswVehicleDescriptor.PerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip;
            tfnswVehicleDescriptor.SpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
        }

        var vehicle = new VehicleDescriptorDto
        {
            Id = vehiclePosition.Vehicle.Id,
            Label = vehiclePosition.Vehicle.Label,
            LicensePlate = vehiclePosition.Vehicle.LicensePlate,
            TfnswVehicleDescriptor = tfnswVehicleDescriptor,
        };

        var position = new PositionDto
        {
            Latitude = vehiclePosition.Position.Latitude,
            Longitude = vehiclePosition.Position.Longitude,
            Bearing = vehiclePosition.Position.Bearing,
            Odometer = vehiclePosition.Position.Odometer,
            Speed = vehiclePosition.Position.Speed,
            TrackDirection = RealtimeEnums.MapTrackDirection(vehiclePosition.Position.GetExtension(GtfsRealtime1007ExtensionExtensions.TrackDirection))
        };

        var currentStopSequence = vehiclePosition.CurrentStopSequence;
        var stopId = vehiclePosition.StopId;
        var currentStatus = RealtimeEnums.MapVehicleStopStatus(vehiclePosition.CurrentStatus);
        var timestamp = vehiclePosition.Timestamp;
        var congestionLevel = RealtimeEnums.MapCongestionLevel(vehiclePosition.CongestionLevel);
        var occupancyStatus = RealtimeEnums.MapOccupancyStatusVehiclePosition(vehiclePosition.OccupancyStatus);

        var consist = new List<CarriageDescriptorDto>();
        var consistExtension = vehiclePosition.GetExtension(GtfsRealtime1007ExtensionExtensions.Consist);
        if (consistExtension != null)
        {
            foreach (var cd in consistExtension)
            {
                consist.Add(new CarriageDescriptorDto
                {
                    Name = cd.Name,
                    PositionInConsist = cd.PositionInConsist,
                    OccupancyStatus = RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cd.OccupancyStatus),
                    QuietCarriage = cd.QuietCarriage,
                    Toilet = RealtimeEnums.MapToiletStatus(cd.Toilet),
                    LuggageRack = cd.LuggageRack,
                    DepartureOccupancyStatus = RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cd.DepartureOccupancyStatus)
                });
            }
        }
        
        return new VehiclePositionDto
        {
            Trip = trip,
            Vehicle = vehicle,
            Position = position,
            CurrentStopSequence = currentStopSequence,
            StopId = stopId,
            CurrentStatus = currentStatus,
            Timestamp = timestamp,
            CongestionLevel = congestionLevel,
            OccupancyStatus = occupancyStatus,
            Consist = consist,
        };
    }
}
