using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("trips")]
public class Trip
{
    [Key]
    [Column("trip_id")]
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;

    [Column("route_id")]
    [Required]
    [StringLength(255)]
    public string RouteId { get; set; } = null!;

    [Column("service_id")]
    [Required]
    public int ServiceId { get; set; }

    [ForeignKey("ServiceId")]
    public Calendar? Service { get; set; }

    [Column("shape_id")]
    [Required]
    public int ShapeId { get; set; }

    [Column("trip_headsign")]
    [Required]
    [StringLength(255)]
    public string HeadSign { get; set; } = null!;

    [Column("direction_id")]
    [Required]
    public bool DirectionId { get; set; } // TINYINT(1) → bool

    [Column("trip_short_name")]
    [Required]
    [StringLength(255)]
    public string ShortName { get; set; } = null!;

    [Column("block_id")]
    [StringLength(255)]
    public string? BlockId { get; set; }

    [Column("wheelchair_accessible")]
    [Required]
    public bool WheelchairAccessible { get; set; }

    [Column("trip_note")]
    [StringLength(255)]
    public string? TripNote { get; set; }

    [Column("route_direction")]
    [Required]
    public string RouteDirection { get; set; } = null!;

    [Column("bikes_allowed")]
    public bool? BikesAllowed { get; set; }
}
