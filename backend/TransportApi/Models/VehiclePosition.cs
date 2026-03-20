using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("vehicle_positions")]
public class VehiclePosition
{
    [Column("trip_id")]
    public string? TripId { get; set; }

    [Column("trip_route_id")]
    public string? TripRouteId { get; set; }

    [Column("trip_direction_id")]
    public uint? TripDirectionId { get; set; }

    [Column("trip_start_time")]
    public string? TripStartTime { get; set; }

    [Column("trip_start_date")]
    public string? TripStartDate { get; set; }

    [Column("trip_schedule_relationship")]
    public int? TripScheduleRelationship { get; set; }

    [Column("vehicle_id")]
    public string? VehicleId { get; set; }

    [Column("vehicle_label")]
    public string? VehicleLabel { get; set; }

    [Column("vehicle_license_plate")]
    public string? VehicleLicensePlate { get; set; }

    [Column("vehicle_air_conditioned")]
    public int? VehicleAirConditioned { get; set; }

    [Column("vehicle_wheelchair_accessible")]
    public int? VehicleWheelchairAccessible { get; set; }

    [Column("vehicle_model")]
    public string? VehicleModel { get; set; }

    [Column("vehicle_performing_prior_trip")]
    public int? VehiclePerformingPriorTrip { get; set; }

    [Column("vehicle_special_vehicle_attributes")]
    public int? VehicleSpecialVehicleAttributes { get; set; }

    [Column("position_latitude")]
    public double? PositionLatitude { get; set; }

    [Column("position_longitude")]
    public double? PositionLongitude { get; set; }

    [Column("position_bearing")]
    public double? PositionBearing { get; set; }

    [Column("position_odometer")]
    public double? PositionOdometer { get; set; }

    [Column("position_speed")]
    public double? PositionSpeed { get; set; }

    [Column("position_track_direction")]
    public int? PositionTrackDirection { get; set; }

    [Column("current_stop_sequence")]
    public uint? CurrentStopSequence { get; set; }

    [Column("stop_id")]
    public string? StopId { get; set; }

    [Column("current_status")]    
    public int? CurrentStatus { get; set; }

    [Column("timestamp")]
    public ulong? Timestamp { get; set; }

    [Column("congestion_level")]
    public int? CongestionLevel { get; set; }

    [Column("occupancy_status")]
    public int? OccupancyStatus { get; set; }

    [Column("stu_carriage_name")]
    public string? StuCarriageName { get; set; }

    [Column("stu_carriage_position_in_consist")]
    public int? StuCarriagePositionInConsist { get; set; }    

    [Column("stu_carriage_occupancy_status")]
    public int? StuCarriageOccupancyStatus { get; set; }

    [Column("stu_carriage_quiet_carriage")]
    public int? StuCarriageQuietCarriage { get; set; }

    [Column("stu_carriage_toilet")]
    public int? StuCarriageToilet { get; set; }

    [Column("stu_carriage_luggage_rack")]
    public int? StuCarriageLuggageRack { get; set; }

    [Column("stu_carriage_departure_occupancy_status")]
    public int? StuCarriageDepartureOccupancyStatus { get; set; }

    public static List<VehiclePosition> Parse(TransitRealtime.VehiclePosition vehiclePosition, string mode)
    {
        var newVehiclePositions = new List<VehiclePosition>();

        var consistExtension = vehiclePosition.GetOrInitializeExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.Consist);
        if (consistExtension != null)
        {
            var index = 0;
            if (vehiclePosition.Trip.DirectionId == 0) index = 5;
            foreach (var cd in consistExtension)
            {
                var newVehiclePosition = new VehiclePosition
                {
                    TripId = vehiclePosition.Trip.TripId,
                    TripRouteId = vehiclePosition.Trip.RouteId,
                    TripDirectionId = vehiclePosition.Trip.DirectionId,
                    TripStartTime = vehiclePosition.Trip.StartTime,
                    TripStartDate = vehiclePosition.Trip.StartDate,
                    TripScheduleRelationship = (int?)Realtime.RealtimeEnums.MapScheduleRelationshipTripDescriptor(vehiclePosition.Trip.ScheduleRelationship),

                    VehicleId = vehiclePosition.Vehicle.Id,
                    VehicleLabel = vehiclePosition.Vehicle.Label,
                    VehicleLicensePlate = vehiclePosition.Vehicle.LicensePlate,

                    PositionLatitude = vehiclePosition.Position.Latitude,
                    PositionLongitude = vehiclePosition.Position.Longitude,
                    PositionBearing = vehiclePosition.Position.Bearing,
                    PositionOdometer = vehiclePosition.Position.Odometer,
                    PositionSpeed = vehiclePosition.Position.Speed,
                    PositionTrackDirection = (int?)Realtime.RealtimeEnums.MapTrackDirection(vehiclePosition.Position.GetExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TrackDirection)),

                    CurrentStopSequence = vehiclePosition.CurrentStopSequence,
                    StopId = vehiclePosition.StopId,
                    CurrentStatus = (int?)Realtime.RealtimeEnums.MapVehicleStopStatus(vehiclePosition.CurrentStatus),
                    Timestamp = vehiclePosition.Timestamp,
                    CongestionLevel = (int?)Realtime.RealtimeEnums.MapCongestionLevel(vehiclePosition.CongestionLevel),
                    OccupancyStatus = (int?)Realtime.RealtimeEnums.MapOccupancyStatusVehiclePosition(vehiclePosition.OccupancyStatus),

                    StuCarriageName = cd.Name,
                    StuCarriageOccupancyStatus = (int?)Realtime.RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cd.OccupancyStatus),
                    StuCarriageQuietCarriage = cd.QuietCarriage ? 1 : 0,
                    StuCarriageToilet = (int?)Realtime.RealtimeEnums.MapToiletStatus(cd.Toilet),
                    StuCarriageLuggageRack = cd.LuggageRack ? 1 : 0,
                    StuCarriageDepartureOccupancyStatus = (int?)Realtime.RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cd.DepartureOccupancyStatus)
                };

                if (mode == "Metro")
                {
                    newVehiclePosition.StuCarriagePositionInConsist = newVehiclePosition.TripDirectionId == 0 ?  index-- : index++;
                }
                else
                {
                    newVehiclePosition.StuCarriagePositionInConsist = cd.PositionInConsist;
                }

                if (vehiclePosition.Vehicle.HasExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
                {
                    var tfnswVehicleDescriptorExtension = vehiclePosition.Vehicle.GetExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
                    newVehiclePosition.VehicleAirConditioned = tfnswVehicleDescriptorExtension.AirConditioned ? 1 : 0;
                    newVehiclePosition.VehicleWheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
                    newVehiclePosition.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
                    newVehiclePosition.VehiclePerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip ? 1 : 0;
                    newVehiclePosition.VehicleSpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
                }

                newVehiclePositions.Add(newVehiclePosition);
            }
        }
        else
        {
            var newVehiclePosition = new VehiclePosition
            {
                TripId = vehiclePosition.Trip.TripId,
                TripRouteId = vehiclePosition.Trip.RouteId,
                TripDirectionId = vehiclePosition.Trip.DirectionId,
                TripStartTime = vehiclePosition.Trip.StartTime,
                TripStartDate = vehiclePosition.Trip.StartDate,
                TripScheduleRelationship = (int?)Realtime.RealtimeEnums.MapScheduleRelationshipTripDescriptor(vehiclePosition.Trip.ScheduleRelationship),

                VehicleId = vehiclePosition.Vehicle.Id,
                VehicleLabel = vehiclePosition.Vehicle.Label,
                VehicleLicensePlate = vehiclePosition.Vehicle.LicensePlate,

                PositionLatitude = vehiclePosition.Position.Latitude,
                PositionLongitude = vehiclePosition.Position.Longitude,
                PositionBearing = vehiclePosition.Position.Bearing,
                PositionOdometer = vehiclePosition.Position.Odometer,
                PositionSpeed = vehiclePosition.Position.Speed,
                PositionTrackDirection = (int?)Realtime.RealtimeEnums.MapTrackDirection(vehiclePosition.Position.GetExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TrackDirection)),

                CurrentStopSequence = vehiclePosition.CurrentStopSequence,
                StopId = vehiclePosition.StopId,
                CurrentStatus = (int?)Realtime.RealtimeEnums.MapVehicleStopStatus(vehiclePosition.CurrentStatus),
                Timestamp = vehiclePosition.Timestamp,
                CongestionLevel = (int?)Realtime.RealtimeEnums.MapCongestionLevel(vehiclePosition.CongestionLevel),
                OccupancyStatus = (int?)Realtime.RealtimeEnums.MapOccupancyStatusVehiclePosition(vehiclePosition.OccupancyStatus),
            };

            if (vehiclePosition.Vehicle.HasExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
            {
                var tfnswVehicleDescriptorExtension = vehiclePosition.Vehicle.GetExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
                newVehiclePosition.VehicleAirConditioned = tfnswVehicleDescriptorExtension.AirConditioned ? 1 : 0;
                newVehiclePosition.VehicleWheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
                newVehiclePosition.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
                newVehiclePosition.VehiclePerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip ? 1 : 0;
                newVehiclePosition.VehicleSpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
            }

            newVehiclePositions.Add(newVehiclePosition);
        }

        return newVehiclePositions;
    }
}
