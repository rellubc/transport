using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class StopTimeDto
{
    [Required]
    [StringLength(255)]
    public string TripId { get; set; } = null!;

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan DepartureTime { get; set; }

    public string StopId { get; set; } = null!;

    public string StopSequence { get; set; } = null!;

    [StringLength(255)]
    public string? StopHeadSign { get; set; }

    public bool PickupType { get; set; }

    public bool DropOffType { get; set; }

    [Precision(18, 2)]
    public decimal ShapeDistanceTravelled { get; set; }

    public bool Timepoint { get; set; }

    [StringLength(255)]
    public string? StopNote { get; set; }
}