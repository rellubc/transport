using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("occupancy")]
public class Occupancy
{
    [Column("trip_id")]
    [Required]
    public string TripId { get; set; } = null!;

    [Column("stop_sequence")]
    [Required]
    public string StopSequence { get; set; } = null!;

    [Column("occupancy_status")]
    [Required]
    public int OccupancyStatus { get; set; }

    [Column("monday")]
    [Required]
    public bool Monday { get; set; }

    [Column("tuesday")]
    [Required]
    public bool Tuesday { get; set; }

    [Column("wednesday")]
    [Required]
    public bool Wednesday { get; set; }

    [Column("thursday")]
    [Required]
    public bool Thursday { get; set; }

    [Column("friday")]
    [Required]
    public bool Friday { get; set; }

    [Column("saturday")]
    [Required]
    public bool Saturday { get; set; }

    [Column("sunday")]
    [Required]
    public bool Sunday { get; set; }

    [Column("start_date")]
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
    
    [Column("exception")]
    public bool? Exception { get; set; }
}