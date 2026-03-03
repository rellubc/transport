using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.DTOs;

[Table("realtime_vehicle_positions")]
public class RealtimeVehiclePositionDto
{
    [Column("entity_id")]
    [Required]
    [StringLength(255)]
    public string EntityId { get; set; } = null!;

    [Column("vehicle_id")]
    [StringLength(255)]
    public string? VehicleId { get; set; }

    [Column("label")]
    [StringLength(255)]
    public string? Label { get; set; }

    [Column("license_plate")]
    [StringLength(255)]
    public string? LicensePlate { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("bearing")]
    public decimal? Bearing { get; set; }

    [Column("speed")]
    public decimal? Speed { get; set; }

    [Column("trip_id")]
    [StringLength(255)]
    public string? TripId { get; set; }

    [Column("current_stop_sequence")]
    public uint? CurrentStopSequence { get; set; }

    [Column("stop_id")]
    public int? StopId { get; set; }

    [Column("current_status")]
    [StringLength(100)]
    public string? CurrentStatus { get; set; }

    [Column("timestamp")]
    public DateTime? Timestamp { get; set; }

    [Column("congestion_level")]
    [StringLength(100)]
    public string? CongestionLevel { get; set; }

    [Column("occupancy_status")]
    [StringLength(100)]
    public string? OccupancyStatus { get; set; }

    [Column("inserted_at")]
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
}
