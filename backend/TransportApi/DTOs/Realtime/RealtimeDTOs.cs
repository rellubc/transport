namespace TransportApi.DTOs.Realtime;

public class RealtimeTripUpdateDto
{
    public TripDescriptorDto Trip { get; set; } = null!;
    public VehicleDescriptorDto? Vehicle { get; set; }
    public List<StopTimeUpdateDto> StopTimeUpdate { get; set; } = [];
    public ulong? Timestamp { get; set; }
    public int? Delay { get; set; }
}

public class RealtimeVehiclePositionDto
{
    public VehicleDescriptorDto? Vehicle { get; set; }
    public PositionDto? Position { get; set; }
    public TripDescriptorDto? Trip { get; set; }
    public uint? CurrentStopSequence { get; set; }
    public string? StopId { get; set; }
    public RealtimeEnums.VehicleStopStatusEnum? CurrentStatus { get; set; } = RealtimeEnums.VehicleStopStatusEnum.IN_TRANSIT_TO;
    public ulong? Timestamp { get; set; }
    public RealtimeEnums.CongestionLevelEnum? CongestionLevel { get; set; }
    public RealtimeEnums.OccupancyStatusEnum? OccupancyStatus { get; set; }
    public CarriageDescriptorDto? Consist { get; set; }
}

public class TripDescriptorDto
{
    public string? TripId { get; set; }
    public string? RouteId { get; set; }
    public uint? DirectionId { get; set; }
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
    public RealtimeEnums.ScheduleRelationshipTripDescriptorEnum? ScheduleRelationship { get; set; }
}

public class VehicleDescriptorDto
{
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string? LicensePlate { get; set; }
    public TfnswVehicleDescriptorDto? TfnswVehicleDescriptor { get; set; }
}

public class PositionDto
{
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public float? Bearing { get; set; }
    public double? Odometer { get; set; }
    public float? Speed { get; set; }
    public RealtimeEnums.TrackDirectionEnum? TrackDirection { get; set; }
}

public class StopTimeUpdateDto
{
    public uint? StopSequence { get; set; }
    public string? StopId { get; set; }
    public StopTimeEventDto? Arrival { get; set; }
    public StopTimeEventDto? Departure { get; set; }
    public RealtimeEnums.ScheduleRelationshipStopTimeUpdateEnum? ScheduleRelationship { get; set; } = RealtimeEnums.ScheduleRelationshipStopTimeUpdateEnum.SCHEDULED;
    public RealtimeEnums.OccupancyStatusEnum? DepartureOccupancyStatus { get; set; }
    public CarriageDescriptorDto? CarriageSeqPredictiveOccupancy { get; set; }
}

public class StopTimeEventDto
{
    public int? Delay { get; set; }
    public long? Time { get; set; }
    public int? Uncertainty { get; set; }
}

public class CarriageDescriptorDto
{
    public string? Name { get; set; }
    public int PositionInConsist { get; set; }
    public RealtimeEnums.OccupancyStatusEnum? OccupancyStatus { get; set; }
    public bool? QuietCarriage { get; set; } = false;
    public RealtimeEnums.ToiletStatusEnum? Toilet { get; set; }
    public bool? LuggageRack { get; set; } = false;
    public RealtimeEnums.OccupancyStatusEnum? DepartureOccupancyStatus { get; set; }
}

public class TfnswVehicleDescriptorDto
{
    public bool? AirConditioned { get; set; }
    public int? WheelchairAccessible { get; set; } = 0;
    public string? VehicleModel { get; set; }
    public bool? PerformingPriorTrip { get; set; } = false;
    public int? SpecialVehicleAttributes { get; set; } = 0;
}