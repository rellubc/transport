using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("trip_updates")]
public class TripUpdate
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

    [Column("stu_stop_sequence")]
    public uint? StuStopSequence { get; set; }

    [Column("stu_stop_id")]
    public string? StuStopId { get; set; }

    [Column("stu_arrival_delay")]
    public long? StuArrivalDelay { get; set; }

    [Column("stu_arrival_time")]
    public long? StuArrivalTime { get; set; }

    [Column("stu_arrival_uncertainty")]
    public int? StuArrivalUncertainty { get; set; }

    [Column("stu_departure_delay")]
    public long? StuDepartureDelay { get; set; }

    [Column("stu_departure_time")]
    public long? StuDepartureTime { get; set; }

    [Column("stu_departure_uncertainty")]
    public int? StuDepartureUncertainty { get; set; }

    [Column("stu_schedule_relationship")]
    public int? StuScheduleRelationship { get; set; }

    [Column("stu_departure_occupancy_status")]
    public int? StuDepartureOccupancyStatus { get; set; }

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

    [Column("timestamp")]
    public ulong? Timestamp { get; set; }

    [Column("delay")]
    public int? Delay { get; set; }

    public static List<TripUpdate> Parse(TransitRealtime.TripUpdate tripUpdate, string mode)
    {
        var newTripUpdates = new List<TripUpdate>();

        foreach (var stu in tripUpdate.StopTimeUpdate)
        {
            var CarriageSeqPredictiveOccupancyExtension = stu.GetExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.CarriageSeqPredictiveOccupancy);
            if (CarriageSeqPredictiveOccupancyExtension != null)
            {
                var index = 0;
                if (tripUpdate.Trip.DirectionId == 0) index = 5;
                foreach (var cspo in CarriageSeqPredictiveOccupancyExtension)
                {
                    var newTripUpdate = new TripUpdate()
                    {
                        TripId = tripUpdate.Trip.TripId,
                        TripRouteId = tripUpdate.Trip.RouteId,
                        TripDirectionId = tripUpdate.Trip.DirectionId,
                        TripStartTime = tripUpdate.Trip.StartTime,
                        TripStartDate = tripUpdate.Trip.StartDate,
                        TripScheduleRelationship = (int?)Realtime.RealtimeEnums.MapScheduleRelationshipTripDescriptor(tripUpdate.Trip.ScheduleRelationship),

                        VehicleId = tripUpdate.Vehicle.Id,
                        VehicleLabel = tripUpdate.Vehicle.Label,
                        VehicleLicensePlate = tripUpdate.Vehicle.LicensePlate,

                        Timestamp = tripUpdate.Timestamp,
                        Delay = tripUpdate.Delay,

                        StuCarriageName = cspo.Name,
                        StuCarriageOccupancyStatus = (int?)Realtime.RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cspo.OccupancyStatus),
                        StuCarriageQuietCarriage = cspo.QuietCarriage ? 1 : 0,
                        StuCarriageToilet = (int?)Realtime.RealtimeEnums.MapToiletStatus(cspo.Toilet),
                        StuCarriageLuggageRack = cspo.LuggageRack ? 1 : 0,
                        StuCarriageDepartureOccupancyStatus = (int?)Realtime.RealtimeEnums.MapOccupancyStatusCarriageDescriptor(cspo.DepartureOccupancyStatus)
                    };

                    if (mode == "Metro")
                    {
                        newTripUpdate.StuCarriagePositionInConsist = newTripUpdate.TripDirectionId == 0 ?  index-- : index++;
                    }
                    else
                    {
                        newTripUpdate.StuCarriagePositionInConsist = cspo.PositionInConsist;
                    }
                    
                    newTripUpdate.StuStopSequence = stu.StopSequence;
                    newTripUpdate.StuStopId = stu.StopId;

                    if (stu.Arrival != null) {
                        newTripUpdate.StuArrivalDelay = stu.Arrival.Delay;
                        newTripUpdate.StuArrivalDelay = stu.Arrival.Time;
                        newTripUpdate.StuArrivalDelay = stu.Arrival.Uncertainty;
                    }

                    if (stu.Departure != null) {
                        newTripUpdate.StuDepartureDelay = stu.Departure.Delay;
                        newTripUpdate.StuDepartureTime = stu.Departure.Time;
                        newTripUpdate.StuDepartureUncertainty = stu.Departure.Uncertainty;
                    }

                    if (tripUpdate.Vehicle.HasExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
                    {
                        var tfnswVehicleDescriptorExtension = tripUpdate.Vehicle.GetExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
                        newTripUpdate.VehicleAirConditioned = tfnswVehicleDescriptorExtension.AirConditioned ? 1 : 0;
                        newTripUpdate.VehicleWheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
                        newTripUpdate.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
                        newTripUpdate.VehiclePerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip ? 1 : 0;
                        newTripUpdate.VehicleSpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
                    }

                    newTripUpdates.Add(newTripUpdate);
                }
            }
            else
            {
                var newTripUpdate = new TripUpdate()
                {
                    TripId = tripUpdate.Trip.TripId,
                    TripRouteId = tripUpdate.Trip.RouteId,
                    TripDirectionId = tripUpdate.Trip.DirectionId,
                    TripStartTime = tripUpdate.Trip.StartTime,
                    TripStartDate = tripUpdate.Trip.StartDate,
                    TripScheduleRelationship = (int?)Realtime.RealtimeEnums.MapScheduleRelationshipTripDescriptor(tripUpdate.Trip.ScheduleRelationship),

                    VehicleId = tripUpdate.Vehicle.Id,
                    VehicleLabel = tripUpdate.Vehicle.Label,
                    VehicleLicensePlate = tripUpdate.Vehicle.LicensePlate,

                    Timestamp = tripUpdate.Timestamp,
                    Delay = tripUpdate.Delay,
                };

                if (stu.Arrival != null) {
                    newTripUpdate.StuArrivalDelay = stu.Arrival.Delay;
                    newTripUpdate.StuArrivalDelay = stu.Arrival.Time;
                    newTripUpdate.StuArrivalDelay = stu.Arrival.Uncertainty;
                }

                if (stu.Departure != null) {
                    newTripUpdate.StuDepartureDelay = stu.Departure.Delay;
                    newTripUpdate.StuDepartureTime = stu.Departure.Time;
                    newTripUpdate.StuDepartureUncertainty = stu.Departure.Uncertainty;
                }

                if (tripUpdate.Vehicle.HasExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor))
                {
                    var tfnswVehicleDescriptorExtension = tripUpdate.Vehicle.GetExtension(TransitRealtime.GtfsRealtime1007ExtensionExtensions.TfnswVehicleDescriptor);
                    newTripUpdate.VehicleAirConditioned = tfnswVehicleDescriptorExtension.AirConditioned ? 1 : 0;
                    newTripUpdate.VehicleWheelchairAccessible = tfnswVehicleDescriptorExtension.WheelchairAccessible;
                    newTripUpdate.VehicleModel = tfnswVehicleDescriptorExtension.VehicleModel;
                    newTripUpdate.VehiclePerformingPriorTrip = tfnswVehicleDescriptorExtension.PerformingPriorTrip ? 1 : 0;
                    newTripUpdate.VehicleSpecialVehicleAttributes = tfnswVehicleDescriptorExtension.SpecialVehicleAttributes;
                }

                newTripUpdates.Add(newTripUpdate);
            }
        }

        return newTripUpdates;
    }
}