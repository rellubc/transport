using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public static Stop ParseColumns(string mode, string[] cols)
    {
        var stop = new Stop
        {
            Id = cols[0],
        };

        var index = cols[2].IndexOf("n Platform");
		if (index != -1) cols[2] = $"{cols[2][.. (index + 1)]},{cols[2][(index + 1)..]}";

        if (mode == "metro")
        {
            stop.Name = cols[1];
            stop.Latitude = decimal.Parse(cols[2]);
            stop.Longitude = decimal.Parse(cols[3]);
            stop.LocationType = int.Parse(cols[4]);
            stop.ParentStationId = string.IsNullOrWhiteSpace(cols[5]) ? null : cols[5];
            stop.WheelchairBoarding = int.Parse(cols[6]);
            stop.PlatformCode = string.IsNullOrWhiteSpace(cols[7]) ? null : int.Parse(cols[7]);
            stop.Mode = "Metro";
        }
        else if (mode == "sydneytrains")
        {
            stop.Code = cols[1];
            stop.Name = cols[2];
            stop.Description = cols[3];
            stop.Latitude = decimal.Parse(cols[4]);
            stop.Longitude = decimal.Parse(cols[5]);
            stop.ZoneId = cols[6];
            stop.Url = cols[7];
            stop.LocationType = int.Parse(cols[8]);
            stop.ParentStationId = string.IsNullOrWhiteSpace(cols[9]) ? null : cols[9];
            stop.Timezone = cols[10];
            stop.WheelchairBoarding = int.Parse(cols[11]);
            stop.Mode = "Rail";
        }

        return stop;
    }
}
