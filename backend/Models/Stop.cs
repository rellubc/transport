using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("stops")]
public class Stop
{
    [Required]
    [Column("stop_id")]
    public int StopId { get; set; }
    
    [Column("stop_name")]
    [Required]
    public string StopName { get; set; } = null!;
    
    [Column("stop_lat")]
    [Required]
    public float StopLat { get; set; }
    
    [Column("stop_lon")]
    [Required]
    public float StopLon { get; set; }
    
    [Column("location_type")]
    [Required]
    public string LocationType { get; set; } = null!;
    
    [Column("parent_station")]
    public int? ParentStation { get; set; }
    
    [Column("wheelchair_boarding")]
    [Required]
    public bool WheelchairBoarding { get; set; }
    
    [Column("platform_code")]
    public int? PlatformCode { get; set; }
}
