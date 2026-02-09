using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class StopTimeDto
{
    [Required]
    [StringLength(255)]
    public string TripId { get; set; } = null!;

    [Required]
    public TimeSpan ArrivalTime { get; set; }

    [Required]
    public TimeSpan DepartureTime { get; set; }

    [Required]
    public int StopId { get; set; }

    [Required]
    public int StopSequence { get; set; }

    [StringLength(255)]
    public string? StopHeadSign { get; set; }

    [Required]
    public bool PickupType { get; set; }

    [Required]
    public bool DropOffType { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal ShapeDistanceTraveled { get; set; }

    [Required]
    public bool Timepoint { get; set; }

    [StringLength(255)]
    public string? StopNote { get; set; }
}