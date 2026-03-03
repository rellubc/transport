using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class TripDto
{
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string RouteId { get; set; } = null!;

    public int ServiceId { get; set; }

    public int ShapeId { get; set; }

    [Required]
    [StringLength(255)]
    public string HeadSign { get; set; } = null!;

    public bool DirectionId { get; set; } // TINYINT(1) → bool

    [Required]
    [StringLength(255)]
    public string ShortName { get; set; } = null!;

    [StringLength(255)]
    public string? BlockId { get; set; }

    public bool WheelchairAccessible { get; set; }

    [StringLength(255)]
    public string? TripNote { get; set; }

    [Required]
    public string RouteDirection { get; set; } = null!;

    public bool? BikesAllowed { get; set; }
}