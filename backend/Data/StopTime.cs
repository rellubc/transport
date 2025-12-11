using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("stop_times")]
    public class StopTime
    {
        [Column("trip_id")]
        [Required]
        public string TripId { get; set; } = null!;
        
        [Column("arrival_time")]
        [Required]
        public TimeOnly ArrivalTime { get; set; }
        
        [Column("departure_time")]
        [Required]
        public TimeOnly DepartureTime { get; set; }
        
        [Column("stop_id")]
        [Required]
        public string StopId { get; set; } = null!;
        
        [Column("stop_sequence")]
        [Required]
        public int StopSequence { get; set; }
        
        [Column("stop_headsign")]
        [Required]
        public string StopHeadsign { get; set; } = null!;
        
        [Column("pickup_type")]
        [Required]
        public bool PickupType { get; set; }
        
        [Column("drop_off_type")]
        [Required]
        public bool DropOffType { get; set; }
        
        [Column("shape_dist_traveled")]
        [Required]
        public float ShapeDistTraveled { get; set; }
    }
}
