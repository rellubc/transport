using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

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
    public int? PlatformCode { get; set; }

    [Column("mode")]
    public string Mode { get; set; } = null!;

    public static Stop ParseMetroColumns(string[] cols)
    {
        return new Stop
        {
            Id = cols[0],
            Name = cols[1],
            Latitude = decimal.Parse(cols[2]),
            Longitude = decimal.Parse(cols[3]),
            LocationType = int.Parse(cols[4]),
            ParentStationId = string.IsNullOrWhiteSpace(cols[5]) ? null : cols[5],
            WheelchairBoarding = int.Parse(cols[6]),
            PlatformCode = string.IsNullOrWhiteSpace(cols[7]) ? null : int.Parse(cols[7]),
            Mode = "Metro"
        };
    }

    public static Stop ParseRailColumns(string[] cols)
    {
        return new Stop
        {
            Id = cols[0],
            Code = cols[1],
            Name = cols[2],
            Description = cols[3],
            Latitude = decimal.Parse(cols[4]),
            Longitude = decimal.Parse(cols[5]),
            ZoneId = cols[6],
            Url = cols[7],
            LocationType = int.Parse(cols[8]),
            ParentStationId = string.IsNullOrWhiteSpace(cols[9]) ? null : cols[9],
            Timezone = cols[10],
            WheelchairBoarding = int.Parse(cols[11]),
            Mode = "Rail"
        };
    }
}
