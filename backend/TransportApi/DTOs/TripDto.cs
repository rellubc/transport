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

    public string ServiceId { get; set; } = null!;

    public string ShapeId { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string HeadSign { get; set; } = null!;

    public int DirectionId { get; set; }

    [Required]
    [StringLength(255)]
    public string ShortName { get; set; } = null!;

    [StringLength(255)]
    public string? BlockId { get; set; }

    public bool WheelchairAccessible { get; set; }

    [StringLength(255)]
    public string? TripNote { get; set; }

    [StringLength(255)]
    public string? RouteDirection { get; set; }

    public bool? BikesAllowed { get; set; }
    public string? VehicleCategoryId { get; set; }
}