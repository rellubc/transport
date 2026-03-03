using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("realtime_stop_time_updates")]
public class RealtimeStopTimeUpdate
{
    [Column("entity_id")]
    [Required]
    [StringLength(255)]
    public string EntityId { get; set; } = null!;

    [Column("trip_id")]
    [Required]
    [StringLength(255)]
    public string TripId { get; set; } = null!;

    [Column("stop_sequence")]
    public int? StopSequence { get; set; }

    [Column("stop_id")]
    public int? StopId { get; set; }

    [Column("arrival_time")]
    public DateTime? ArrivalTime { get; set; }

    [Column("departure_time")]
    public DateTime? DepartureTime { get; set; }

    [Column("schedule_relationship")]
    [StringLength(50)]
    public string? ScheduleRelationship { get; set; }

    [Column("inserted_at")]
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
}
