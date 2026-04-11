package models

type VehicleStopStatus int
type CongestionLevel int
type OccupancyStatus int

type ScheduleRelationship int

const (
	INCOMING_AT VehicleStopStatus = iota
	STOPPED_AT
	IN_TRANSIT_TO
)

const (
	UNKNOWN_CONGESTION_LEVEL CongestionLevel = iota
	RUNNING_SMOOTHLY
	STOP_AND_GO
	CONGESTION
	SEVERE_CONGESTION
)

const (
	EMPTY OccupancyStatus = iota
	MANY_SEATS_AVAILABLE
	FEW_SEATS_AVAILABLE
	STANDING_ROOM_ONLY
	CRUSHED_STANDING_ROOM_ONLY
	FULL
	NOT_ACCEPTING_PASSENGERS
)

const (
	SCHEDULED ScheduleRelationship = iota
	ADDED
	UNSCHEDULED
	CANCELED
	REPLACEMENT
)

func ParseVehicleStopStatus(s string) VehicleStopStatus {
	switch s {
	case "INCOMING_AT":
		return INCOMING_AT
	case "STOPPED_AT":
		return STOPPED_AT
	default:
		return IN_TRANSIT_TO
	}
}

func ParseCongestionLevel(s string) CongestionLevel {
	switch s {
	case "RUNNING_SMOOTHLY":
		return RUNNING_SMOOTHLY
	case "STOP_AND_GO":
		return STOP_AND_GO
	case "CONGESTION":
		return CONGESTION
	case "SEVERE_CONGESTION":
		return SEVERE_CONGESTION
	default:
		return UNKNOWN_CONGESTION_LEVEL
	}
}

func ParseOccupancyStatus(s string) OccupancyStatus {
	switch s {
	case "MANY_SEATS_AVAILABLE":
		return MANY_SEATS_AVAILABLE
	case "FEW_SEATS_AVAILABLE":
		return FEW_SEATS_AVAILABLE
	case "STANDING_ROOM_ONLY":
		return STANDING_ROOM_ONLY
	case "CRUSHED_STANDING_ROOM_ONLY":
		return CRUSHED_STANDING_ROOM_ONLY
	case "FULL":
		return FULL
	case "NOT_ACCEPTING_PASSENGERS":
		return NOT_ACCEPTING_PASSENGERS
	default:
		return EMPTY
	}
}

func ParseScheduleRelationship(s string) ScheduleRelationship {
	switch s {
	case "ADDED":
		return ADDED
	case "UNSCHEDULED":
		return UNSCHEDULED
	case "CANCELED":
		return CANCELED
	case "REPLACEMENT":
		return REPLACEMENT
	default:
		return SCHEDULED
	}
}
