using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportStatic.Models;

[Table("trips")]
public class Trip
{
    [Key]
    [Column("trip_id")]
    public string TripId { get; set; } = null!;

    [Column("route_id")]
    public string RouteId { get; set; } = null!;

    [Column("service_id")]
    public string ServiceId { get; set; } = null!;

    [Column("shape_id")]
    public string ShapeId { get; set; } = null!;

    [Column("trip_headsign")]
    public string? TripHeadsign { get; set; }

    [Column("direction_id")]
    public int DirectionId { get; set; }

    [Column("trip_short_name")]
    public string? TripShortName { get; set; }

    [Column("block_id")]
    public string? BlockId { get; set; }

    [Column("wheelchair_accessible")]
    public int WheelchairAccessible { get; set; }

    [Column("trip_note")]
    public string? TripNote { get; set; }

    [Column("route_direction")]
    public string? RouteDirection { get; set; }

    [Column("bikes_allowed")]
    public int? BikesAllowed { get; set; }

    [Column("vehicle_category_id")]
    public string? VehicleCategoryId { get; set; }
}
