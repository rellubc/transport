using TransitRealtime;

namespace TransportApi.DTOs.Realtime;

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






    public static CauseEnum? MapCause(TransitRealtime.Alert.Types.Cause c) 
    {
        return c switch
        {
            TransitRealtime.Alert.Types.Cause.UnknownCause => CauseEnum.UNKNOWN_CAUSE,
            TransitRealtime.Alert.Types.Cause.OtherCause => CauseEnum.OTHER_CAUSE,
            TransitRealtime.Alert.Types.Cause.TechnicalProblem => CauseEnum.TECHNICAL_PROBLEM,
            TransitRealtime.Alert.Types.Cause.Strike => CauseEnum.STRIKE,
            TransitRealtime.Alert.Types.Cause.Demonstration => CauseEnum.DEMONSTRATION,
            TransitRealtime.Alert.Types.Cause.Accident => CauseEnum.ACCIDENT,
            TransitRealtime.Alert.Types.Cause.Holiday => CauseEnum.HOLIDAY,
            TransitRealtime.Alert.Types.Cause.Weather => CauseEnum.WEATHER,
            TransitRealtime.Alert.Types.Cause.Maintenance => CauseEnum.MAINTENANCE,
            TransitRealtime.Alert.Types.Cause.Construction => CauseEnum.CONSTRUCTION,
            TransitRealtime.Alert.Types.Cause.PoliceActivity => CauseEnum.POLICE_ACTIVITY,
            _ => null
        };
    }

    public static CongestionLevelEnum? MapCongestionLevel(TransitRealtime.VehiclePosition.Types.CongestionLevel cl) 
    {
        return cl switch
        {
            TransitRealtime.VehiclePosition.Types.CongestionLevel.UnknownCongestionLevel => CongestionLevelEnum.UNKNOWN_CONGESTION_LEVEL,
            TransitRealtime.VehiclePosition.Types.CongestionLevel.RunningSmoothly => CongestionLevelEnum.RUNNING_SMOOTHLY,
            TransitRealtime.VehiclePosition.Types.CongestionLevel.StopAndGo => CongestionLevelEnum.STOP_AND_GO,
            TransitRealtime.VehiclePosition.Types.CongestionLevel.Congestion => CongestionLevelEnum.CONGESTION,
            TransitRealtime.VehiclePosition.Types.CongestionLevel.SevereCongestion => CongestionLevelEnum.SEVERE_CONGESTION,
            _ => null
        };
    }

    public static EffectEnum? MapEffect(TransitRealtime.Alert.Types.Effect e) 
    {
        return e switch
        {
            TransitRealtime.Alert.Types.Effect.NoService => EffectEnum.NO_SERVICE,
            TransitRealtime.Alert.Types.Effect.ReducedService => EffectEnum.REDUCED_SERVICE,
            TransitRealtime.Alert.Types.Effect.SignificantDelays => EffectEnum.SIGNIFICANT_DELAYS,
            TransitRealtime.Alert.Types.Effect.Detour => EffectEnum.DETOUR,
            TransitRealtime.Alert.Types.Effect.AdditionalService => EffectEnum.ADDITIONAL_SERVICE,
            TransitRealtime.Alert.Types.Effect.ModifiedService => EffectEnum.MODIFIED_SERVICE,
            TransitRealtime.Alert.Types.Effect.OtherEffect => EffectEnum.OTHER_EFFECT,
            TransitRealtime.Alert.Types.Effect.UnknownEffect => EffectEnum.UNKNOWN_EFFECT,
            TransitRealtime.Alert.Types.Effect.StopMoved => EffectEnum.STOP_MOVED,
            TransitRealtime.Alert.Types.Effect.NoEffect => EffectEnum.NO_EFFECT,
            TransitRealtime.Alert.Types.Effect.AccessibilityIssue => EffectEnum.ACCESSIBILITY_ISSUE,
            _ => null
        };
    }

    public static IncrementalityEnum? MapIncrementality(TransitRealtime.FeedHeader.Types.Incrementality i) 
    {
        return i switch
        {
            TransitRealtime.FeedHeader.Types.Incrementality.FullDataset => IncrementalityEnum.FULL_DATASET,
            TransitRealtime.FeedHeader.Types.Incrementality.Differential => IncrementalityEnum.DIFFERENTIAL,
            _ => null
        };
    }

    public static OccupancyStatusEnum? MapOccupancyStatusStopTimeUpdate(TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus os) 
    {
        return os switch
        {
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.Empty => OccupancyStatusEnum.EMPTY,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.ManySeatsAvailable => OccupancyStatusEnum.MANY_SEATS_AVAILABLE,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.FewSeatsAvailable => OccupancyStatusEnum.FEW_SEATS_AVAILABLE,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.StandingRoomOnly => OccupancyStatusEnum.STANDING_ROOM_ONLY,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.CrushedStandingRoomOnly => OccupancyStatusEnum.CRUSHED_STANDING_ROOM_ONLY,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.Full => OccupancyStatusEnum.FULL,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.OccupancyStatus.NotAcceptingPassengers => OccupancyStatusEnum.NOT_ACCEPTING_PASSENGERS,
            _ => null
        };
    }

    public static OccupancyStatusEnum? MapOccupancyStatusVehiclePosition(TransitRealtime.VehiclePosition.Types.OccupancyStatus os) 
    {
        return os switch
        {
            TransitRealtime.VehiclePosition.Types.OccupancyStatus.Empty => OccupancyStatusEnum.EMPTY,
            TransitRealtime.VehiclePosition.Types.OccupancyStatus.ManySeatsAvailable => OccupancyStatusEnum.MANY_SEATS_AVAILABLE,
            TransitRealtime.VehiclePosition.Types.OccupancyStatus.FewSeatsAvailable => OccupancyStatusEnum.FEW_SEATS_AVAILABLE,
            TransitRealtime.VehiclePosition.Types.OccupancyStatus.StandingRoomOnly => OccupancyStatusEnum.STANDING_ROOM_ONLY,
            TransitRealtime.VehiclePosition.Types.OccupancyStatus.CrushedStandingRoomOnly => OccupancyStatusEnum.CRUSHED_STANDING_ROOM_ONLY,
            TransitRealtime.VehiclePosition.Types.OccupancyStatus.Full => OccupancyStatusEnum.FULL,
            TransitRealtime.VehiclePosition.Types.OccupancyStatus.NotAcceptingPassengers => OccupancyStatusEnum.NOT_ACCEPTING_PASSENGERS,
            _ => null
        };
    }

    public static OccupancyStatusEnum? MapOccupancyStatusCarriageDescriptor(TransitRealtime.CarriageDescriptor.Types.OccupancyStatus os) 
    {
        return os switch
        {
            TransitRealtime.CarriageDescriptor.Types.OccupancyStatus.Empty => OccupancyStatusEnum.EMPTY,
            TransitRealtime.CarriageDescriptor.Types.OccupancyStatus.ManySeatsAvailable => OccupancyStatusEnum.MANY_SEATS_AVAILABLE,
            TransitRealtime.CarriageDescriptor.Types.OccupancyStatus.FewSeatsAvailable => OccupancyStatusEnum.FEW_SEATS_AVAILABLE,
            TransitRealtime.CarriageDescriptor.Types.OccupancyStatus.StandingRoomOnly => OccupancyStatusEnum.STANDING_ROOM_ONLY,
            TransitRealtime.CarriageDescriptor.Types.OccupancyStatus.CrushedStandingRoomOnly => OccupancyStatusEnum.CRUSHED_STANDING_ROOM_ONLY,
            TransitRealtime.CarriageDescriptor.Types.OccupancyStatus.Full => OccupancyStatusEnum.FULL,
            _ => null
        };
    }

    public static ScheduleRelationshipStopTimeUpdateEnum? MapScheduleRelationshipStopTimeUpdate(TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship sr) 
    {
        return sr switch
        {
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.Scheduled => ScheduleRelationshipStopTimeUpdateEnum.SCHEDULED,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.Skipped => ScheduleRelationshipStopTimeUpdateEnum.SKIPPED,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.NoData => ScheduleRelationshipStopTimeUpdateEnum.NO_DATA,
            TransitRealtime.TripUpdate.Types.StopTimeUpdate.Types.ScheduleRelationship.Unscheduled => ScheduleRelationshipStopTimeUpdateEnum.UNSCHEDULED,            
            _ => null
        };
    }

    public static ScheduleRelationshipTripDescriptorEnum? MapScheduleRelationshipTripDescriptor(TransitRealtime.TripDescriptor.Types.ScheduleRelationship sr) 
    {
        return sr switch
        {
            TransitRealtime.TripDescriptor.Types.ScheduleRelationship.Scheduled => ScheduleRelationshipTripDescriptorEnum.SCHEDULED,
            TransitRealtime.TripDescriptor.Types.ScheduleRelationship.Added => ScheduleRelationshipTripDescriptorEnum.ADDED,
            TransitRealtime.TripDescriptor.Types.ScheduleRelationship.Unscheduled => ScheduleRelationshipTripDescriptorEnum.UNSCHEDULED,
            TransitRealtime.TripDescriptor.Types.ScheduleRelationship.Canceled => ScheduleRelationshipTripDescriptorEnum.CANCELED,
            TransitRealtime.TripDescriptor.Types.ScheduleRelationship.Replacement => ScheduleRelationshipTripDescriptorEnum.REPLACEMENT,
            _ => null
        };
    }

    public static SeverityLevelEnum? MapSeverityLevel(TransitRealtime.Alert.Types.SeverityLevel sl) 
    {
        return sl switch
        {
            TransitRealtime.Alert.Types.SeverityLevel.UnknownSeverity => SeverityLevelEnum.UNKNOWN_SEVERITY,
            TransitRealtime.Alert.Types.SeverityLevel.Info => SeverityLevelEnum.INFO,
            TransitRealtime.Alert.Types.SeverityLevel.Warning => SeverityLevelEnum.WARNING,
            TransitRealtime.Alert.Types.SeverityLevel.Severe => SeverityLevelEnum.SEVERE,
            _ => null
        };
    }

    public static ToiletStatusEnum? MapToiletStatus(TransitRealtime.CarriageDescriptor.Types.ToiletStatus ts) 
    {
        return ts switch
        {
            TransitRealtime.CarriageDescriptor.Types.ToiletStatus.None => ToiletStatusEnum.NONE,
            TransitRealtime.CarriageDescriptor.Types.ToiletStatus.Normal => ToiletStatusEnum.NORMAL,
            TransitRealtime.CarriageDescriptor.Types.ToiletStatus.Accessible => ToiletStatusEnum.ACCESSIBLE,
            _ => null
        };
    }

    public static TrackDirectionEnum? MapTrackDirection(TransitRealtime.TrackDirection td) 
    {
        return td switch
        {
            TransitRealtime.TrackDirection.Up => TrackDirectionEnum.UP,
            TransitRealtime.TrackDirection.Down => TrackDirectionEnum.DOWN,
            _ => null
        };
    }

    public static VehicleStopStatusEnum? MapVehicleStopStatus(TransitRealtime.VehiclePosition.Types.VehicleStopStatus vss) 
    {
        return vss switch
        {
            TransitRealtime.VehiclePosition.Types.VehicleStopStatus.IncomingAt => VehicleStopStatusEnum.INCOMING_AT,
            TransitRealtime.VehiclePosition.Types.VehicleStopStatus.StoppedAt => VehicleStopStatusEnum.STOPPED_AT,
            TransitRealtime.VehiclePosition.Types.VehicleStopStatus.InTransitTo => VehicleStopStatusEnum.IN_TRANSIT_TO,
            _ => null
        };
    }
}