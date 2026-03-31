using TransportStatic.DTOs.Realtime;

using Google.Protobuf;

using TransitRealtime;
using static TransitRealtime.TripUpdate.Types;

namespace TransportStatic.Services;

public class RealtimeService(IHttpClientFactory factory, ILogger<RealtimeService> logger) : IRealtimeService
{
    private readonly IHttpClientFactory _factory = factory;
    private readonly ILogger<RealtimeService> _logger = logger;

    public async Task<TripUpdateDTO> GetRealtimeTripUpdate(string mode, string tripId)
    {
        _logger.LogInformation("Updating vehicle trip details...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync($"https://api.transport.nsw.gov.au/v2/gtfs/realtime/{Common.Mappings.UrlMappings[mode]}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return new TripUpdateDTO();
        }

        ExtensionRegistry registry = [GtfsRealtime1007ExtensionExtensions.Update, GtfsRealtime1007ExtensionExtensions.Consist, GtfsRealtime1007ExtensionExtensions.TrackDirection, GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor, GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy];
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var parser = FeedMessage.Parser.WithExtensionRegistry(registry);
        var feed = parser.ParseFrom(responseStream);

        var newTripUpdate = new TripUpdateDTO();

        if (feed == null) return new TripUpdateDTO();

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

    public async Task<List<VehiclePositionDTO>> GetRealtimeVehicles(string mode)
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

        var newVehicles = new List<VehiclePositionDTO>();

        if (feed == null) return [];

        foreach (var entity in feed.Entity)
        {
            try
            {
                if (entity.Vehicle == null) continue;
                newVehicles.Add(TransformVehicle(entity.Vehicle));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process vehicle entity {EntityId}", entity.Id);
            }
        }
        return newVehicles;
    }

    private async Task<TripUpdateDTO> TransformTripUpdate(TripUpdate tripUpdate)
    {
        var trip = new TripDescriptorDTO
        {
            TripId = tripUpdate.Trip.TripId,
            RouteId = tripUpdate.Trip.RouteId,
            DirectionId = tripUpdate.Trip.DirectionId,
            StartTime = tripUpdate.Trip.StartTime,
            StartDate = tripUpdate.Trip.StartDate,
            ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipTripDescriptor(tripUpdate.Trip.ScheduleRelationship) 
        };

        var tfnswVehicleDescriptor = new TfnswVehicleDescriptorDTO();
        if (tripUpdate.Vehicle.HasExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
        {
            TransitRealtime.TfnswVehicleDescriptor tfnswVehicleDescriptorExtension = tripUpdate.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
            tfnswVehicleDescriptor.AirConditioned = tfnswVehicleDescriptorExtension.AirConditioned;
            tfnswVehicleDescriptor.WheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
            tfnswVehicleDescriptor.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
            tfnswVehicleDescriptor.PerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip;
            tfnswVehicleDescriptor.SpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
        }

        var vehicle = new VehicleDescriptorDTO
        {
            Id = tripUpdate.Vehicle.Id,
            Label = tripUpdate.Vehicle.Label,
            LicensePlate = tripUpdate.Vehicle.LicensePlate,
            TfnswVehicleDescriptor = tfnswVehicleDescriptor,
        };

        var newStopTimeUpdates = new List<StopTimeUpdateDTO>();
        foreach (var stu in tripUpdate.StopTimeUpdate)
        {
            var arrival = new StopTimeEventDTO();
            if (stu.Arrival != null) {
                arrival.Delay = stu.Arrival.Delay;
                arrival.Time = stu.Arrival.Time;
                arrival.Uncertainty = stu.Arrival.Uncertainty;
            }

            var departure = new StopTimeEventDTO();
            if (stu.Departure != null) {
                departure.Delay = stu.Departure.Delay;
                departure.Time = stu.Departure.Time;
                departure.Uncertainty = stu.Departure.Uncertainty;
            }

            var carriageSeqPredictiveOccupancy = new List<CarriageDescriptorDTO>();
            var CarriageSeqPredictiveOccupancyExtension = stu.GetExtension(GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy);
            if (CarriageSeqPredictiveOccupancyExtension != null)
            {
                foreach (var cspo in CarriageSeqPredictiveOccupancyExtension)
                {
                    carriageSeqPredictiveOccupancy.Add(new CarriageDescriptorDTO
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

            newStopTimeUpdates.Add(new StopTimeUpdateDTO
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

        return new TripUpdateDTO
        {
            Trip = trip,
            Vehicle = vehicle,
            StopTimeUpdate = newStopTimeUpdates,
            Timestamp = timestamp,
            Delay = delay
        };
    }

    private static VehiclePositionDTO TransformVehicle(VehiclePosition vehiclePosition)
    {
        var trip = new TripDescriptorDTO();
        if (vehiclePosition.Trip != null)
        {
            trip.TripId = vehiclePosition.Trip.TripId;
            trip.RouteId = vehiclePosition.Trip.RouteId;
            trip.DirectionId = vehiclePosition.Trip.DirectionId;
            trip.StartTime = vehiclePosition.Trip.StartTime;
            trip.StartDate = vehiclePosition.Trip.StartDate;
            trip.ScheduleRelationship = RealtimeEnums.MapScheduleRelationshipTripDescriptor(vehiclePosition.Trip.ScheduleRelationship);
        }

        var tfnswVehicleDescriptor = new TfnswVehicleDescriptorDTO();
        if (vehiclePosition.Vehicle.HasExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
        {
            var tfnswVehicleDescriptorExtension = vehiclePosition.Vehicle.GetExtension(GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
            tfnswVehicleDescriptor.AirConditioned = tfnswVehicleDescriptorExtension.AirConditioned;
            tfnswVehicleDescriptor.WheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
            tfnswVehicleDescriptor.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
            tfnswVehicleDescriptor.PerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip;
            tfnswVehicleDescriptor.SpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
        }

        var vehicleDescriptor = new VehicleDescriptorDTO
        {
            Id = vehiclePosition.Vehicle.Id,
            Label = vehiclePosition.Vehicle.Label,
            LicensePlate = vehiclePosition.Vehicle.LicensePlate,
            TfnswVehicleDescriptor = tfnswVehicleDescriptor,
        };

        var position = new PositionDTO
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

        var consist = new List<CarriageDescriptorDTO>();
        var consistExtension = vehiclePosition.GetExtension(GtfsRealtime1007ExtensionExtensions.Consist);
        if (consistExtension != null)
        {
            foreach (var cd in consistExtension)
            {
                consist.Add(new CarriageDescriptorDTO
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
        
        return new VehiclePositionDTO
        {
            Trip = trip,
            Vehicle = vehicleDescriptor,
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
















    public async Task<List<TripUpdate>> GetRealtimeTripUpdates(string mode)
    {
        _logger.LogInformation("Updating vehicle trip details...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync($"https://api.transport.nsw.gov.au/v2/gtfs/realtime/{Common.Mappings.UrlMappings[mode]}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return [];
        }

        ExtensionRegistry registry = [GtfsRealtime1007ExtensionExtensions.Update, GtfsRealtime1007ExtensionExtensions.Consist, GtfsRealtime1007ExtensionExtensions.TrackDirection, GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor, GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy];
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var parser = FeedMessage.Parser.WithExtensionRegistry(registry);
        var feed = parser.ParseFrom(responseStream);

        if (feed == null) return [];

        var tripUpdates = new List<TripUpdate>();
        foreach (var entity in feed.Entity)
        {
            if (entity.TripUpdate != null) tripUpdates.Add(entity.TripUpdate);
        }
        _logger.LogInformation($"{tripUpdates.Count} trip updates");

        return tripUpdates;
    }

    public async Task<List<VehiclePosition>> GetRealtimeVehiclePositions(string mode)
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

        if (feed == null) return [];

        var vehiclePositions = new List<VehiclePosition>();
        foreach (var entity in feed.Entity)
        {
            if (entity.Vehicle != null) vehiclePositions.Add(entity.Vehicle);
        }
        _logger.LogInformation($"{vehiclePositions.Count} vehicle positions");

        return vehiclePositions;
    }
}
