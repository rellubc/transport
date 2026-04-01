using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace TransportStatic.Models;

[Table("stops")]
public class Stop
{
    [Column("stop_id")]
    public string Id { get; set; } = null!;

    [Column("stop_code")]
    public string? Code { get; set; }

    [Column("stop_name")]
    public string Name { get; set; } = null!;

    [Column("stop_desc")]
    public string? Description { get; set; }

    [Column("stop_lat")]
    public decimal Latitude { get; set; }

    [Column("stop_lon")]
    public decimal Longitude { get; set; }

    [Column("geom")]
    public Point Geom { get; set; } = null!;

    [Column("zone_id")]
    public string? ZoneId { get; set; }

    [Column("stop_url")]
    public string? Url { get; set; }

    [Column("location_type")]
    public int LocationType { get; set; }

    [Column("parent_station")]
    public string? ParentStationId { get; set; }

    [Column("stop_timezone")]
    public string? Timezone { get; set; }

    [Column("wheelchair_boarding")]
    public int WheelchairBoarding { get; set; }

    [Column("platform_code")]
    public string? PlatformCode { get; set; }

    [Column("mode")]
    public string Mode { get; set; } = null!;
}
