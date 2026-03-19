using TransportApi.Data;
using TransportApi.Models.Realtime;

using Google.Protobuf;

using TransitRealtime;

namespace TransportApi.Services;

public interface ISydneyService
{
    Task<Models.Realtime.TripUpdate> SydneyRealtimeTripUpdates(string TripId, string mode);
    Task<List<Models.Realtime.VehiclePosition>> SydneyRealtimeVehiclePositions(string mode);
}

public class SydneyService(TransportDbContext db, IHttpClientFactory factory, ILogger<SydneyService> logger) : ISydneyService
{
    private readonly IHttpClientFactory _factory = factory;
    private readonly TransportDbContext _db = db;
    private readonly ILogger<SydneyService> _logger = logger;

    public async Task<Models.Realtime.TripUpdate> SydneyRealtimeTripUpdates(string mode, string tripId)
    {
        _logger.LogInformation("Updating vehicle trip details...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync($"https://api.transport.nsw.gov.au/v2/gtfs/realtime/{Common.Mappings.UrlMappings[mode]}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return new Models.Realtime.TripUpdate();
        }

        ExtensionRegistry registry = [GtfsRealtime1007ExtensionExtensions.Update, GtfsRealtime1007ExtensionExtensions.Consist, GtfsRealtime1007ExtensionExtensions.TrackDirection, GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor, GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy];
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var parser = FeedMessage.Parser.WithExtensionRegistry(registry);
        var feed = parser.ParseFrom(responseStream);

        var newTripUpdate = new Models.Realtime.TripUpdate();

        if (feed == null) return new Models.Realtime.TripUpdate();

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

    public async Task<List<Models.Realtime.VehiclePosition>> SydneyRealtimeVehiclePositions(string mode)
    {
        _logger.LogInformation("Updating vehicle positions...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync($"https://api.transport.nsw.gov.au/v2/gtfs/vehiclepos/{Common.Mappings.UrlMappings[mode]}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return [];
        }

        ExtensionRegistry registry = [GtfsRealtime1007ExtensionExtensions.Update, GtfsRealtime1007ExtensionExtensions.Consist, GtfsRealtime1007ExtensionExtensions.TrackDirection, GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor, GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy];
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var parser = FeedMessage.Parser.WithExtensionRegistry(registry);
        var feed = parser.ParseFrom(responseStream);

        var newVehiclePositions = new List<Models.Realtime.VehiclePosition>();

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

    private async Task<Models.Realtime.TripUpdate> TransformTripUpdate(TransitRealtime.TripUpdate tripUpdate)
    {
        var trip = new Models.Realtime.TripDescriptor
        {
            TripId = tripUpdate.Trip.TripId,
            RouteId = tripUpdate.Trip.RouteId,
            DirectionId = tripUpdate.Trip.DirectionId,
            StartTime = tripUpdate.Trip.StartTime,
            StartDate = tripUpdate.Trip.StartDate,
            ScheduleRelationship = Models.Realtime.RealtimeEnums.MapScheduleRelationshipTripDescriptor(tripUpdate.Trip.ScheduleRelationship) 
        };

        var tfnswVehicleDescriptor = new Models.Realtime.TfnswVehicleDescriptor();
        if (tripUpdate.Vehicle.HasExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
        {
            TransitRealtime.TfnswVehicleDescriptor tfnswVehicleDescriptorExtension = tripUpdate.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
            tfnswVehicleDescriptor.AirConditioned = tfnswVehicleDescriptorExtension.AirConditioned;
            tfnswVehicleDescriptor.WheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
            tfnswVehicleDescriptor.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
            tfnswVehicleDescriptor.PerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip;
            tfnswVehicleDescriptor.SpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
        }

        var vehicle = new Models.Realtime.VehicleDescriptor
        {
            Id = tripUpdate.Vehicle.Id,
            Label = tripUpdate.Vehicle.Label,
            LicensePlate = tripUpdate.Vehicle.LicensePlate,
            TfnswVehicleDescriptor = tfnswVehicleDescriptor,
        };

        var newStopTimeUpdates = new List<Models.Realtime.StopTimeUpdate>();
        foreach (var stu in tripUpdate.StopTimeUpdate)
        {
            var arrival = new Models.Realtime.StopTimeEvent();
            if (stu.Arrival != null) {
                arrival.Delay = stu.Arrival.Delay;
                arrival.Time = stu.Arrival.Time;
                arrival.Uncertainty = stu.Arrival.Uncertainty;
            }

            var departure = new Models.Realtime.StopTimeEvent();
            if (stu.Departure != null) {
                departure.Delay = stu.Departure.Delay;
                departure.Time = stu.Departure.Time;
                departure.Uncertainty = stu.Departure.Uncertainty;
            }

            var carriageSeqPredictiveOccupancy = new List<Models.Realtime.CarriageDescriptor>();
            var CarriageSeqPredictiveOccupancyExtension = stu.GetExtension(GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy);
            if (CarriageSeqPredictiveOccupancyExtension != null)
            {
                foreach (var cspo in CarriageSeqPredictiveOccupancyExtension)
                {
                    carriageSeqPredictiveOccupancy.Add(new Models.Realtime.CarriageDescriptor
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

            newStopTimeUpdates.Add(new Models.Realtime.StopTimeUpdate
            {
                StopSequence = stu.StopSequence,
                StopId = stu.StopId,
                Arrival = arrival,
                Departure = departure,
                ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipStopTimeUpdate(stu.ScheduleRelationship),
                DepartureOccupancyStatus = RealtimeEnums.MapOccupancyStatusStopTimeUpdate(stu.DepartureOccupancyStatus),
                CarriageSeqPredictiveOccupancy = carriageSeqPredictiveOccupancy,
            });
        }

        var timestamp = tripUpdate.Timestamp;
        var delay = tripUpdate.Delay;

        return new Models.Realtime.TripUpdate
        {
            Trip = trip,
            Vehicle = vehicle,
            StopTimeUpdate = newStopTimeUpdates,
            Timestamp = timestamp,
            Delay = delay
        };
    }

    private static Models.Realtime.VehiclePosition TransformVehiclePosition(TransitRealtime.VehiclePosition vehiclePosition)
    {
        var trip = new Models.Realtime.TripDescriptor();
        if (vehiclePosition.Trip != null)
        {
            trip.TripId = vehiclePosition.Trip.TripId;
            trip.RouteId = vehiclePosition.Trip.RouteId;
            trip.DirectionId = vehiclePosition.Trip.DirectionId;
            trip.StartTime = vehiclePosition.Trip.StartTime;
            trip.StartDate = vehiclePosition.Trip.StartDate;
            trip.ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipTripDescriptor(vehiclePosition.Trip.ScheduleRelationship);
        }

        var tfnswVehicleDescriptor = new Models.Realtime.TfnswVehicleDescriptor();
        if (vehiclePosition.Vehicle.HasExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
        {
            var tfnswVehicleDescriptorExtension = vehiclePosition.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
            tfnswVehicleDescriptor.AirConditioned = tfnswVehicleDescriptorExtension.AirConditioned;
            tfnswVehicleDescriptor.WheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
            tfnswVehicleDescriptor.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
            tfnswVehicleDescriptor.PerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip;
            tfnswVehicleDescriptor.SpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
        }

        var vehicle = new Models.Realtime.VehicleDescriptor
        {
            Id = vehiclePosition.Vehicle.Id,
            Label = vehiclePosition.Vehicle.Label,
            LicensePlate = vehiclePosition.Vehicle.LicensePlate,
            TfnswVehicleDescriptor = tfnswVehicleDescriptor,
        };

        var position = new Models.Realtime.Position
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

        var consist = new List<Models.Realtime.CarriageDescriptor>();
        var consistExtension = vehiclePosition.GetExtension(GtfsRealtime1007ExtensionExtensions.Consist);
        if (consistExtension != null)
        {
            foreach (var cd in consistExtension)
            {
                consist.Add(new Models.Realtime.CarriageDescriptor
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
        
        return new Models.Realtime.VehiclePosition
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
