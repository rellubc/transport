using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class StopDto
{
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    [StringLength(255)]
    public string LocationType { get; set; } = null!;

    public string? ParentStationId { get; set; }

    public bool WheelchairBoarding { get; set; }

    public int? PlatformCode { get; set; }
}