using TransitRealtime;

namespace TransportStatic.DTOs.Realtime;

public static class RealtimeEnums
{
    public enum CauseEnum {
        UNKNOWN_CAUSE = 1,
        OTHER_CAUSE = 2,
        TECHNICAL_PROBLEM = 3,
        STRIKE = 4,
        DEMONSTRATION = 5,
        ACCIDENT = 6,
        HOLIDAY = 7,
        WEATHER = 8,
        MAINTENANCE = 9,
        CONSTRUCTION = 10,
        POLICE_ACTIVITY = 11,
        MEDICAL_EMERGENCY = 12,
    }
    public enum CongestionLevelEnum {
        UNKNOWN_CONGESTION_LEVEL = 0,
        RUNNING_SMOOTHLY = 1,
        STOP_AND_GO = 2,
        CONGESTION = 3,
        SEVERE_CONGESTION = 4,
    }

    public enum EffectEnum {
        NO_SERVICE = 1,
        REDUCED_SERVICE = 2,
        SIGNIFICANT_DELAYS = 3,
        DETOUR = 4,
        ADDITIONAL_SERVICE = 5,
        MODIFIED_SERVICE = 6,
        OTHER_EFFECT = 7,
        UNKNOWN_EFFECT = 8,
        STOP_MOVED = 9,
        NO_EFFECT = 10,
        ACCESSIBILITY_ISSUE = 11,
    }

    public enum IncrementalityEnum {
        FULL_DATASET = 0,
        DIFFERENTIAL = 1,
    }

    public enum OccupancyStatusEnum {
        EMPTY = 0,
        MANY_SEATS_AVAILABLE = 1,
        FEW_SEATS_AVAILABLE = 2,
        STANDING_ROOM_ONLY = 3,
        CRUSHED_STANDING_ROOM_ONLY = 4,
        FULL = 5,
        NOT_ACCEPTING_PASSENGERS = 6,
    }


    public enum ScheduleRelationshipStopTimeUpdateEnum {
        SCHEDULED = 0,
        SKIPPED = 1,
        NO_DATA = 2,
        UNSCHEDULED = 3,
    }

    public enum ScheduleRelationshipTripDescriptorEnum {
        SCHEDULED = 0,
        ADDED = 1,
        UNSCHEDULED = 2,
        CANCELED = 3,
        REPLACEMENT = 5, // KEEP IT
    }

    public enum SeverityLevelEnum {
        UNKNOWN_SEVERITY = 1,
        INFO = 2,
        WARNING = 3,
        SEVERE = 4,
    }

    public enum ToiletStatusEnum {
        NONE = 0,
        NORMAL = 1,
        ACCESSIBLE = 2,
    }

    public enum TrackDirectionEnum {
        UP = 0,
        DOWN = 1,
    }

    public enum VehicleStopStatusEnum {
        INCOMING_AT = 0,
        STOPPED_AT = 1,
        IN_TRANSIT_TO = 2,
    }

    public static CauseEnum? MapCause(Alert.Types.Cause c) 
    {
        return c switch
        {
            Alert.Types.Cause.UnknownCause => CauseEnum.UNKNOWN_CAUSE,
            Alert.Types.Cause.OtherCause => CauseEnum.OTHER_CAUSE,
            Alert.Types.Cause.TechnicalProblem => CauseEnum.TECHNICAL_PROBLEM,
            Alert.Types.Cause.Strike => CauseEnum.STRIKE,
            Alert.Types.Cause.Demonstration => CauseEnum.DEMONSTRATION,
            Alert.Types.Cause.Accident => CauseEnum.ACCIDENT,
            Alert.Types.Cause.Holiday => CauseEnum.HOLIDAY,
            Alert.Types.Cause.Weather => CauseEnum.WEATHER,
            Alert.Types.Cause.Maintenance => CauseEnum.MAINTENANCE,
            Alert.Types.Cause.Construction => CauseEnum.CONSTRUCTION,
            Alert.Types.Cause.PoliceActivity => CauseEnum.POLICE_ACTIVITY,
            _ => null
        };
    }

    public static CongestionLevelEnum? MapCongestionLevel(VehiclePosition.Types.CongestionLevel cl) 
    {
        return cl switch
        {
            VehiclePosition.Types.CongestionLevel.UnknownCongestionLevel => CongestionLevelEnum.UNKNOWN_CONGESTION_LEVEL,
            VehiclePosition.Types.CongestionLevel.RunningSmoothly => CongestionLevelEnum.RUNNING_SMOOTHLY,
            VehiclePosition.Types.CongestionLevel.StopAndGo => CongestionLevelEnum.STOP_AND_GO,
            VehiclePosition.Types.CongestionLevel.Congestion => CongestionLevelEnum.CONGESTION,
            VehiclePosition.Types.CongestionLevel.SevereCongestion => CongestionLevelEnum.SEVERE_CONGESTION,
            _ => null
        };
    }

    public static EffectEnum? MapEffect(Alert.Types.Effect e) 
    {
        return e switch
        {
            Alert.Types.Effect.NoService => EffectEnum.NO_SERVICE,
            Alert.Types.Effect.ReducedService => EffectEnum.REDUCED_SERVICE,
            Alert.Types.Effect.SignificantDelays => EffectEnum.SIGNIFICANT_DELAYS,
            Alert.Types.Effect.Detour => EffectEnum.DETOUR,
            Alert.Types.Effect.AdditionalService => EffectEnum.ADDITIONAL_SERVICE,
            Alert.Types.Effect.ModifiedService => EffectEnum.MODIFIED_SERVICE,
            Alert.Types.Effect.OtherEffect => EffectEnum.OTHER_EFFECT,
            Alert.Types.Effect.UnknownEffect => EffectEnum.UNKNOWN_EFFECT,
            Alert.Types.Effect.StopMoved => EffectEnum.STOP_MOVED,
            Alert.Types.Effect.NoEffect => EffectEnum.NO_EFFECT,
            Alert.Types.Effect.AccessibilityIssue => EffectEnum.ACCESSIBILITY_ISSUE,
            _ => null
        };
    }

    public static IncrementalityEnum? MapIncrementality(FeedHeader.Types.Incrementality i) 
    {
        return i switch
        {
            FeedHeader.Types.Incrementality.FullDataset => IncrementalityEnum.FULL_DATASET,
            FeedHeader.Types.Incrementality.Differential => IncrementalityEnum.DIFFERENTIAL,
            _ => null
        };
    }

    public static OccupancyStatusEnum? MapOccupancyStatusStopTimeUpdate(TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus os) 
    {
        return os switch
        {
            TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.Empty => OccupancyStatusEnum.EMPTY,
            TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.ManySeatsAvailable => OccupancyStatusEnum.MANY_SEATS_AVAILABLE,
            TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.FewSeatsAvailable => OccupancyStatusEnum.FEW_SEATS_AVAILABLE,
            TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.StandingRoomOnly => OccupancyStatusEnum.STANDING_ROOM_ONLY,
            TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.CrushedStandingRoomOnly => OccupancyStatusEnum.CRUSHED_STANDING_ROOM_ONLY,
            TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.Full => OccupancyStatusEnum.FULL,
            TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.NotAcceptingPassengers => OccupancyStatusEnum.NOT_ACCEPTING_PASSENGERS,
            _ => null
        };
    }

    public static OccupancyStatusEnum? MapOccupancyStatusVehiclePosition(VehiclePosition.Types.OccupancyStatus os) 
    {
        return os switch
        {
            VehiclePosition.Types.OccupancyStatus.Empty => OccupancyStatusEnum.EMPTY,
            VehiclePosition.Types.OccupancyStatus.ManySeatsAvailable => OccupancyStatusEnum.MANY_SEATS_AVAILABLE,
            VehiclePosition.Types.OccupancyStatus.FewSeatsAvailable => OccupancyStatusEnum.FEW_SEATS_AVAILABLE,
            VehiclePosition.Types.OccupancyStatus.StandingRoomOnly => OccupancyStatusEnum.STANDING_ROOM_ONLY,
            VehiclePosition.Types.OccupancyStatus.CrushedStandingRoomOnly => OccupancyStatusEnum.CRUSHED_STANDING_ROOM_ONLY,
            VehiclePosition.Types.OccupancyStatus.Full => OccupancyStatusEnum.FULL,
            VehiclePosition.Types.OccupancyStatus.NotAcceptingPassengers => OccupancyStatusEnum.NOT_ACCEPTING_PASSENGERS,
            _ => null
        };
    }

    public static OccupancyStatusEnum? MapOccupancyStatusCarriageDescriptor(CarriageDescriptor.Types.OccupancyStatus os) 
    {
        return os switch
        {
            CarriageDescriptor.Types.OccupancyStatus.Empty => OccupancyStatusEnum.EMPTY,
            CarriageDescriptor.Types.OccupancyStatus.ManySeatsAvailable => OccupancyStatusEnum.MANY_SEATS_AVAILABLE,
            CarriageDescriptor.Types.OccupancyStatus.FewSeatsAvailable => OccupancyStatusEnum.FEW_SEATS_AVAILABLE,
            CarriageDescriptor.Types.OccupancyStatus.StandingRoomOnly => OccupancyStatusEnum.STANDING_ROOM_ONLY,
            CarriageDescriptor.Types.OccupancyStatus.CrushedStandingRoomOnly => OccupancyStatusEnum.CRUSHED_STANDING_ROOM_ONLY,
            CarriageDescriptor.Types.OccupancyStatus.Full => OccupancyStatusEnum.FULL,
            _ => null
        };
    }

    public static ScheduleRelationshipStopTimeUpdateEnum? MapScheduleRelationshipStopTimeUpdate(TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship sr) 
    {
        return sr switch
        {
            TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.Scheduled => ScheduleRelationshipStopTimeUpdateEnum.SCHEDULED,
            TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.Skipped => ScheduleRelationshipStopTimeUpdateEnum.SKIPPED,
            TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.NoData => ScheduleRelationshipStopTimeUpdateEnum.NO_DATA,
            TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.Unscheduled => ScheduleRelationshipStopTimeUpdateEnum.UNSCHEDULED,            
            _ => null
        };
    }

    public static ScheduleRelationshipTripDescriptorEnum? MapScheduleRelationshipTripDescriptor(TripDescriptor.Types.ScheduleRelationship sr) 
    {
        return sr switch
        {
            TripDescriptor.Types.ScheduleRelationship.Scheduled => ScheduleRelationshipTripDescriptorEnum.SCHEDULED,
            TripDescriptor.Types.ScheduleRelationship.Added => ScheduleRelationshipTripDescriptorEnum.ADDED,
            TripDescriptor.Types.ScheduleRelationship.Unscheduled => ScheduleRelationshipTripDescriptorEnum.UNSCHEDULED,
            TripDescriptor.Types.ScheduleRelationship.Canceled => ScheduleRelationshipTripDescriptorEnum.CANCELED,
            TripDescriptor.Types.ScheduleRelationship.Replacement => ScheduleRelationshipTripDescriptorEnum.REPLACEMENT,
            _ => null
        };
    }

    public static SeverityLevelEnum? MapSeverityLevel(Alert.Types.SeverityLevel sl) 
    {
        return sl switch
        {
            Alert.Types.SeverityLevel.UnknownSeverity => SeverityLevelEnum.UNKNOWN_SEVERITY,
            Alert.Types.SeverityLevel.Info => SeverityLevelEnum.INFO,
            Alert.Types.SeverityLevel.Warning => SeverityLevelEnum.WARNING,
            Alert.Types.SeverityLevel.Severe => SeverityLevelEnum.SEVERE,
            _ => null
        };
    }

    public static ToiletStatusEnum? MapToiletStatus(CarriageDescriptor.Types.ToiletStatus ts) 
    {
        return ts switch
        {
            CarriageDescriptor.Types.ToiletStatus.None => ToiletStatusEnum.NONE,
            CarriageDescriptor.Types.ToiletStatus.Normal => ToiletStatusEnum.NORMAL,
            CarriageDescriptor.Types.ToiletStatus.Accessible => ToiletStatusEnum.ACCESSIBLE,
            _ => null
        };
    }

    public static TrackDirectionEnum? MapTrackDirection(TrackDirection td) 
    {
        return td switch
        {
            TrackDirection.Up => TrackDirectionEnum.UP,
            TrackDirection.Down => TrackDirectionEnum.DOWN,
            _ => null
        };
    }

    public static VehicleStopStatusEnum? MapVehicleStopStatus(VehiclePosition.Types.VehicleStopStatus vss) 
    {
        return vss switch
        {
            VehiclePosition.Types.VehicleStopStatus.IncomingAt => VehicleStopStatusEnum.INCOMING_AT,
            VehiclePosition.Types.VehicleStopStatus.StoppedAt => VehicleStopStatusEnum.STOPPED_AT,
            VehiclePosition.Types.VehicleStopStatus.InTransitTo => VehicleStopStatusEnum.IN_TRANSIT_TO,
            _ => null
        };
    }
}