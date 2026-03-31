namespace TransportApi.Models.Realtime;

public class FeedEntity
{
    public string Id { get; set; } = null!;
    public bool? IsDeleted { get; set; } = false;
    public TripUpdate? TripUpdate { get; set; }
    public VehiclePosition? Vehicle { get; set; }
    public Alert? Alert { get; set; }
    public UpdateBundle? Update { get; set; }
}

public class TripUpdate
{
    public TripDescriptor Trip { get; set; } = null!;
    public VehicleDescriptor? Vehicle { get; set; }
    public List<StopTimeUpdate> StopTimeUpdate { get; set; } = [];
    public ulong? Timestamp { get; set; }
    public int? Delay { get; set; }
}

public class VehiclePosition
{
    public TripDescriptor? Trip { get; set; }
    public VehicleDescriptor? Vehicle { get; set; }
    public Position? Position { get; set; }
    public uint? CurrentStopSequence { get; set; }
    public string? StopId { get; set; }
    public RealtimeEnums.VehicleStopStatusEnum? CurrentStatus { get; set; } = RealtimeEnums.VehicleStopStatusEnum.IN_TRANSIT_TO;
    public ulong? Timestamp { get; set; }
    public RealtimeEnums.CongestionLevelEnum? CongestionLevel { get; set; }
    public RealtimeEnums.OccupancyStatusEnum? OccupancyStatus { get; set; }
    public List<CarriageDescriptor>? Consist { get; set; }
}

public class Alert
{
    public List<TimeRange> ActivePeriod { get; set; } = [];
    public List<EntitySelector> InformedEntity { get; set; } = [];
    public RealtimeEnums.CauseEnum? Cause { get; set; } = RealtimeEnums.CauseEnum.UNKNOWN_CAUSE;
    public RealtimeEnums.EffectEnum? Effect { get; set; } = RealtimeEnums.EffectEnum.UNKNOWN_EFFECT;
    public TranslatedString? Url { get; set; }
    public TranslatedString? HeaderText { get; set; }
    public TranslatedString? DescriptionText { get; set; }
    public TranslatedString? TtsHeaderText { get; set; }
    public TranslatedString? TtsDescriptionText { get; set; }
    public RealtimeEnums.SeverityLevelEnum? SeverityLevel { get; set; }   
}

public class UpdateBundle
{
    public string GTFSStaticBundle { get; set; } = null!;
    public int UpdateSequence { get; set; }
    public List<string> CancelledTrip { get; set; } = [];
}

public class TripDescriptor
{
    public string? TripId { get; set; }
    public string? RouteId { get; set; }
    public uint? DirectionId { get; set; }
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
    public RealtimeEnums.ScheduleRelationshipTripDescriptorEnum? ScheduleRelationship { get; set; }
}

public class VehicleDescriptor
{
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string? LicensePlate { get; set; }
    public TfnswVehicleDescriptor? TfnswVehicleDescriptor { get; set; }
}

public class Position
{
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public float? Bearing { get; set; }
    public double? Odometer { get; set; }
    public float? Speed { get; set; }
    public RealtimeEnums.TrackDirectionEnum? TrackDirection { get; set; }
}

public class StopTimeUpdate
{
    public uint? StopSequence { get; set; }
    public string? StopName { get; set; }
    public string? StopId { get; set; }
    public StopTimeEvent? Arrival { get; set; }
    public StopTimeEvent? Departure { get; set; }
    public RealtimeEnums.ScheduleRelationshipStopTimeUpdateEnum? ScheduleRelationship { get; set; } = RealtimeEnums.ScheduleRelationshipStopTimeUpdateEnum.SCHEDULED;
    public RealtimeEnums.OccupancyStatusEnum? DepartureOccupancyStatus { get; set; }
    public List<CarriageDescriptor>? CarriageSeqPredictiveOccupancy { get; set; }
}

public class StopTimeEvent
{
    public int? Delay { get; set; }
    public long? Time { get; set; }
    public int? Uncertainty { get; set; }
}

public class CarriageDescriptor
{
    public string? Name { get; set; }
    public int PositionInConsist { get; set; }
    public RealtimeEnums.OccupancyStatusEnum? OccupancyStatus { get; set; }
    public bool? QuietCarriage { get; set; } = false;
    public RealtimeEnums.ToiletStatusEnum? Toilet { get; set; }
    public bool? LuggageRack { get; set; } = false;
    public RealtimeEnums.OccupancyStatusEnum? DepartureOccupancyStatus { get; set; }
}

public class TfnswVehicleDescriptor
{
    public bool? AirConditioned { get; set; }
    public int? WheelchairAccessible { get; set; } = 0;
    public string? VehicleModel { get; set; }
    public bool? PerformingPriorTrip { get; set; } = false;
    public int? SpecialVehicleAttributes { get; set; } = 0;
}

public class TimeRange
{
    public ulong? Start { get; set; }
    public ulong? End { get; set; }
}

public class EntitySelector
{
    public string? AgencyId { get; set; }
    public string? RouteId { get; set; }
    public int? RouteType { get; set; }
    public TripDescriptor? Trip { get; set; }
    public string? StopId { get; set; }
    public uint? DirectionId { get; set; }
}

public class TranslatedString
{
    public List<Translation> Translation { get; set; } = [];
}

public class Translation
{
    public string Text { get; set; } = null!;
    public string? Language { get; set; }
}