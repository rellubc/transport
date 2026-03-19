using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("trips")]
public class Trip
{
    [Key]
    [Column("trip_id")]
    public string Id { get; set; } = null!;

    [Column("route_id")]
    public string RouteId { get; set; } = null!;

    [Column("service_id")]
    public string ServiceId { get; set; } = null!;

    [Column("shape_id")]
    public string ShapeId { get; set; } = null!;

    [Column("trip_headsign")]
    public string HeadSign { get; set; } = null!;

    [Column("direction_id")]
    public int DirectionId { get; set; }

    [Column("trip_short_name")]
    public string ShortName { get; set; } = null!;

    [Column("block_id")]
    public string BlockId { get; set; } = null!;

    [Column("wheelchair_accessible")]
    public int WheelchairAccessible { get; set; }

    [Column("trip_note")]
    public string? TripNote { get; set; }

    [Column("route_direction")]
    public string? RouteDirection { get; set; }

    [Column("bikes_allowed")]
    public int? BikesAllowed { get; set; }

    [Column("vehicle_category_id")]
    public string? VehicleCategoryId { get; set; }

    public static Trip ParseMetroColumns(string[] cols)
    {
        return new Trip
        {
            RouteId = cols[0],
            ServiceId = cols[1],
            Id = cols[2],
            ShapeId = $"M1_{cols[2]}",
            HeadSign = cols[4],
            DirectionId = int.Parse(cols[5]),
            ShortName = cols[6],
            BlockId = cols[7],
            WheelchairAccessible = int.Parse(cols[8]),
            TripNote = string.IsNullOrWhiteSpace(cols[9]) ? null : cols[9],
            RouteDirection = string.IsNullOrWhiteSpace(cols[10]) ? null : cols[10],
            BikesAllowed = string.IsNullOrWhiteSpace(cols[11]) ? null : int.Parse(cols[11])
        };
    }

    public static Trip ParseRailColumns(string[] cols)
    {
        return new Trip
        {
            RouteId = cols[0],
            ServiceId = cols[1],
            Id = cols[2],
            HeadSign = cols[3],
            ShortName = cols[4],
            DirectionId = int.Parse(cols[5]),
            BlockId = cols[6],
            ShapeId = cols[7],
            WheelchairAccessible = int.Parse(cols[8]),
            VehicleCategoryId = string.IsNullOrWhiteSpace(cols[9]) ? null : cols[9],
        };
    }
}
