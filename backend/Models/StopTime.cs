using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("stop_times")]
public class StopTime
{
    [Column("trip_id")]
    [Required]
    [StringLength(255)]
    public string TripId { get; set; } = null!;

    [Column("arrival_time")]
    [Required]
    public TimeSpan ArrivalTime { get; set; }

    [Column("departure_time")]
    [Required]
    public TimeSpan DepartureTime { get; set; }

    [Column("stop_id")]
    [Required]
    public int StopId { get; set; }

    [Column("stop_sequence")]
    [Required]
    public int StopSequence { get; set; }

    [Column("stop_headsign")]
    [StringLength(255)]
    public string? StopHeadSign { get; set; }

    [Column("pickup_type")]
    [Required]
    public bool PickupType { get; set; }

    [Column("drop_off_type")]
    [Required]
    public bool DropOffType { get; set; }

    [Column("shape_dist_travelled")]
    [Required]
    [Precision(18, 2)]
    public decimal ShapeDistanceTravelled { get; set; }

    [Column("timepoint")]
    [Required]
    public bool Timepoint { get; set; }

    [Column("stop_note")]
    [StringLength(255)]
    public string? StopNote { get; set; }
}
