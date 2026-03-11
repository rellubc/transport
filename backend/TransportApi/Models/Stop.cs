using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("stops")]
public class Stop
{
    [Column("stop_id")]
    public string Id { get; set; } = null!;

    [Column("stop_code")]
    [StringLength(255)]
    public string? Code { get; set; }

    [Column("stop_name")]
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("stop_desc")]
    [StringLength(255)]
    public string? Description { get; set; }

    [Column("stop_lat")]
    [Required]
    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Column("stop_lon")]
    [Required]
    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    [Column("zone_id")]
    [StringLength(255)]
    public string? ZoneId { get; set; }

    [Column("stop_url")]
    [StringLength(255)]
    public string? Url { get; set; }

    [Column("location_type")]
    [Required]
    [StringLength(255)]
    public string LocationType { get; set; } = null!;

    [Column("parent_station")]
    public string? ParentStationId { get; set; }

    [Column("stop_timezone")]
    [StringLength(255)]
    public string? Timezone { get; set; }

    [Column("wheelchair_boarding")]
    [Required]
    public bool WheelchairBoarding { get; set; }

    [Column("platform_code")]
    public int? PlatformCode { get; set; }

    [Column("mode")]
    [Required]
    public string Mode { get; set; } = null!;

    [Column("network")]
    public string? Network { get; set; }
}
