using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("stops")]
    public class Stop
    {
        [Required]
        [Column("stop_id")]
        public string StopId { get; set; } = null!;
        
        [Required]
        [Column("stop_code")]
        public string StopCode { get; set; } = null!;
        
        [Column("stop_name")]
        [Required]
        public string StopName { get; set; } = null!;
        
        [Column("stop_desc")]
        [Required]
        public string StopDesc { get; set; } = null!;
        
        [Column("stop_lat")]
        [Required]
        public float StopLat { get; set; }
        
        [Column("stop_lon")]
        [Required]
        public float StopLon { get; set; }
        
        [Column("zone_id")]
        [Required]
        public string ZoneId { get; set; } = null!;
        
        [Column("stop_url")]
        [Required]
        public string StopUrl { get; set; } = null!;
        
        [Column("location_type")]
        [Required]
        public string LocationType { get; set; } = null!;
        
        [Column("parent_station")]
        [Required]
        public string ParentStation { get; set; } = null!;

        [Column("stop_timezone")]
        [Required]
        public string StopTimezone { get; set; } = null!;
        
        [Column("wheelchair_boarding")]
        [Required]
        public string WheelchairBoarding { get; set; } = null!;
    }
}
