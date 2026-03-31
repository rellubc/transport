using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TransportStatic.Models;

[Table("occupancies")]
public class Occupancy
{
    [Column("trip_id")]
    public string TripId { get; set; } = null!;

    [Column("stop_sequence")]
    public int StopSequence { get; set; }
    
    [Column("occupancy_status")]
    public int OccupancyStatus { get; set; }

    [Column("monday")]
    public bool Monday { get; set; }

    [Column("tuesday")]
    public bool Tuesday { get; set; }

    [Column("wednesday")]
    public bool Wednesday { get; set; }

    [Column("thursday")]
    public bool Thursday { get; set; }

    [Column("friday")]
    public bool Friday { get; set; }

    [Column("saturday")]
    public bool Saturday { get; set; }

    [Column("sunday")]
    public bool Sunday { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("exception")]
    public int? Exception { get; set; }
}
