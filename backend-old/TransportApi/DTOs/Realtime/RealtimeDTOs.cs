namespace TransportStatic.DTOs.Realtime;

public class FeedEntityDTO
{
    public string Id { get; set; } = null!;
    public bool? IsDeleted { get; set; } = false;
    public TripUpdateDTO? TripUpdate { get; set; }
    public VehiclePositionDTO? Vehicle { get; set; }
    public AlertDTO? Alert { get; set; }
    public UpdateBundleDTO? Update { get; set; }
}

public class TripUpdateDTO
{
    public TripDescriptorDTO Trip { get; set; } = null!;
    public VehicleDescriptorDTO? Vehicle { get; set; }
    public List<StopTimeUpdateDTO> StopTimeUpdate { get; set; } = [];
    public ulong? Timestamp { get; set; }
    public int? Delay { get; set; }
}

public class VehiclePositionDTO
{
    public TripDescriptorDTO? Trip { get; set; }
    public VehicleDescriptorDTO? Vehicle { get; set; }
    public PositionDTO? Position { get; set; }
    public uint? CurrentStopSequence { get; set; }
    public string? StopId { get; set; }
    public RealtimeEnums.VehicleStopStatusEnum? CurrentStatus { get; set; } = RealtimeEnums.VehicleStopStatusEnum.IN_TRANSIT_TO;
    public ulong? Timestamp { get; set; }
    public RealtimeEnums.CongestionLevelEnum? CongestionLevel { get; set; }
    public RealtimeEnums.OccupancyStatusEnum? OccupancyStatus { get; set; }
    public List<CarriageDescriptorDTO>? Consist { get; set; }
}

public class AlertDTO
{
    public List<TimeRangeDTO> ActivePeriod { get; set; } = [];
    public List<EntitySelectorDTO> InformedEntity { get; set; } = [];
    public RealtimeEnums.CauseEnum? Cause { get; set; } = RealtimeEnums.CauseEnum.UNKNOWN_CAUSE;
    public RealtimeEnums.EffectEnum? Effect { get; set; } = RealtimeEnums.EffectEnum.UNKNOWN_EFFECT;
    public TranslatedStringDTO? Url { get; set; }
    public TranslatedStringDTO? HeaderText { get; set; }
    public TranslatedStringDTO? DescriptionText { get; set; }
    public TranslatedStringDTO? TtsHeaderText { get; set; }
    public TranslatedStringDTO? TtsDescriptionText { get; set; }
    public RealtimeEnums.SeverityLevelEnum? SeverityLevel { get; set; }   
}

public class UpdateBundleDTO
{
    public string GTFSStaticBundle { get; set; } = null!;
    public int UpdateSequence { get; set; }
    public List<string> CancelledTrip { get; set; } = [];
}

public class TripDescriptorDTO
{
    public string? TripId { get; set; }
    public string? RouteId { get; set; }
    public uint? DirectionId { get; set; }
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
    public RealtimeEnums.ScheduleRelationshipTripDescriptorEnum? ScheduleRelationship { get; set; }
}

public class VehicleDescriptorDTO
{
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string? LicensePlate { get; set; }
    public TfnswVehicleDescriptorDTO? TfnswVehicleDescriptor { get; set; }
}

public class PositionDTO
{
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public float? Bearing { get; set; }
    public double? Odometer { get; set; }
    public float? Speed { get; set; }
    public RealtimeEnums.TrackDirectionEnum? TrackDirection { get; set; }
}

public class StopTimeUpdateDTO
{
    public uint? StopSequence { get; set; }
    public string? StopName { get; set; }
    public string? StopId { get; set; }
    public StopTimeEventDTO? Arrival { get; set; }
    public StopTimeEventDTO? Departure { get; set; }
    public RealtimeEnums.ScheduleRelationshipStopTimeUpdateEnum? ScheduleRelationship { get; set; } = RealtimeEnums.ScheduleRelationshipStopTimeUpdateEnum.SCHEDULED;
    public RealtimeEnums.OccupancyStatusEnum? DepartureOccupancyStatus { get; set; }
    public List<CarriageDescriptorDTO>? CarriageSeqPredictiveOccupancy { get; set; }
}

public class StopTimeEventDTO
{
    public int? Delay { get; set; }
    public long? Time { get; set; }
    public int? Uncertainty { get; set; }
}

public class CarriageDescriptorDTO
{
    public string? Name { get; set; }
    public int PositionInConsist { get; set; }
    public RealtimeEnums.OccupancyStatusEnum? OccupancyStatus { get; set; }
    public bool? QuietCarriage { get; set; } = false;
    public RealtimeEnums.ToiletStatusEnum? Toilet { get; set; }
    public bool? LuggageRack { get; set; } = false;
    public RealtimeEnums.OccupancyStatusEnum? DepartureOccupancyStatus { get; set; }
}

public class TfnswVehicleDescriptorDTO
{
    public bool? AirConditioned { get; set; }
    public int? WheelchairAccessible { get; set; } = 0;
    public string? VehicleModel { get; set; }
    public bool? PerformingPriorTrip { get; set; } = false;
    public int? SpecialVehicleAttributes { get; set; } = 0;
}

public class TimeRangeDTO
{
    public ulong? Start { get; set; }
    public ulong? End { get; set; }
}

public class EntitySelectorDTO
{
    public string? AgencyId { get; set; }
    public string? RouteId { get; set; }
    public int? RouteType { get; set; }
    public TripDescriptorDTO? Trip { get; set; }
    public string? StopId { get; set; }
    public uint? DirectionId { get; set; }
}

public class TranslatedStringDTO
{
    public List<TranslationDTO> Translation { get; set; } = [];
}

public class TranslationDTO
{
    public string Text { get; set; } = null!;
    public string? Language { get; set; }
}