using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("trips")]
public class Trip
{
    [Column("route_id")]
    [Required]
    public string RouteId { get; set; } = null!;
    
    [Column("service_id")]
    [Required]
    public int ServiceId { get; set; }
    
    [Column("trip_id")]
    [Required]
    public string TripId { get; set; } = null!;
    
    [Column("shape_id")]
    [Required]
    public int ShapeId { get; set; }
    
    [Column("trip_headsign")]
    [Required]
    public string TripHeadsign { get; set; } = null!;

    [Column("direction_id")]
    [Required]
    public bool DirectionId { get; set; }
    
    [Column("trip_short_name")]
    [Required]
    public string TripShortName { get; set; } = null!;
    
    [Column("block_id")]
    public string BlockId { get; set; } = "";
    
    [Column("wheelchair_accessible")]
    [Required]
    public bool WheelchairAccessible { get; set; }

    [Column("trip_note")]
    public string TripNote { get; set; } = "";

    [Column("route_direction")]
    public string RouteDirection { get; set; } = null!;

    [Column("bikes_allowed")]
    public bool BikesAllowed { get; set; }
}
