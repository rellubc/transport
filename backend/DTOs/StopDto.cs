using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class StopDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Required]
    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    [Required]
    [StringLength(255)]
    public string LocationType { get; set; } = null!;

    public int? ParentStationId { get; set; }

    [Required]
    public bool WheelchairBoarding { get; set; }

    public int? PlatformCode { get; set; }
}