using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TransportStatic.Models;

[Table("stop_times")]
public class StopTime
{
    [Column("trip_id")]
    public string TripId { get; set; } = null!;

    [Column("arrival_time")]
    public int ArrivalTime { get; set; }

    [Column("departure_time")]
    public int DepartureTime { get; set; }

    [Column("stop_id")]
    public string StopId { get; set; } = null!;

    [Column("stop_sequence")]
    public int StopSequence { get; set; }

    [Column("stop_headsign")]
    public string StopHeadSign { get; set; } = null!;

    [Column("pickup_type")]
    public int PickupType { get; set; }

    [Column("drop_off_type")]
    public int DropOffType { get; set; }

    [Column("shape_dist_travelled")]
    public decimal? ShapeDistanceTravelled { get; set; }

    [Column("timepoint")]
    public int? Timepoint { get; set; }

    [Column("stop_note")]
    public string? StopNote { get; set; }
    
    [Column("mode")]
    public string Mode { get; set; } = null!;
}
