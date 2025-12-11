using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("trips")]
    public class Trip
    {
        [Column("route_id")]
        [Required]
        public string RouteId { get; set; } = null!;
        
        [Column("service_id")]
        [Required]
        public string ServiceId { get; set; } = null!;
        
        [Column("trip_id")]
        [Required]
        public string TripId { get; set; } = null!;
        
        [Column("shape_id")]
        [Required]
        public string ShapeId { get; set; } = null!;
        
        [Column("trip_headsign")]
        [Required]
        public string TripHeadsign { get; set; } = null!;
        
        [Column("trip_short_name")]
        [Required]
        public string TripShortName { get; set; } = null!;
        
        [Column("direction_id")]
        [Required]
        public bool DirectionId { get; set; }
        
        [Column("block_id")]
        public string BlockId { get; set; } = null!;
        
        [Column("wheelchair_accessible")]
        [Required]
        public bool WheelchairAccessible { get; set; }

        [Column("vehicle_category_id")]
        [Required]
        public string VehicleCategoryId { get; set; } = null!;
    }
}
