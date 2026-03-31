using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TransportStatic.Models;

[Table("stop_times")]
public class StopTime
{
    [Column("trip_id")]
    public string TripId { get; set; } = null!;

    [Column("arrival_time")]
    public TimeSpan ArrivalTime { get; set; }

    [Column("departure_time")]
    public TimeSpan DepartureTime { get; set; }

    [Column("stop_id")]
    public string StopId { get; set; } = null!;

    [Column("stop_sequence")]
    public int StopSequence { get; set; }

    [Column("stop_headsign")]
    public string StopHeadSign { get; set; } = null!;

    [Column("pickup_type")]
    public int PickupType { get; set; }

    [Column("drop_off_type")]
    public int DropOffType { get; set; }

    [Column("shape_dist_travelled")]
    public decimal? ShapeDistanceTravelled { get; set; }

    [Column("timepoint")]
    public int? Timepoint { get; set; }

    [Column("stop_note")]
    public string? StopNote { get; set; }
    
    [Column("mode")]
    public string Mode { get; set; } = null!;

    public static StopTime ParseColumns(string mode, string[] cols)
    {
        int hour1 = int.Parse(cols[1][..2]);
        int hour2 = int.Parse(cols[2][..2]);
        cols[1] = string.Concat((hour1 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[1].AsSpan(2));
        cols[2] = string.Concat((hour2 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[2].AsSpan(2));

        var stopTime = new StopTime
        {
            TripId = cols[0],
            ArrivalTime = TimeSpan.ParseExact(cols[1], @"hh\:mm\:ss", CultureInfo.InvariantCulture),
            DepartureTime = TimeSpan.ParseExact(cols[2], @"hh\:mm\:ss", CultureInfo.InvariantCulture),
            StopId = cols[3],
            StopSequence = int.Parse(cols[4]),
            StopHeadSign = cols[5],
            PickupType = int.Parse(cols[6]),
            DropOffType = int.Parse(cols[7]),
            ShapeDistanceTravelled = string.IsNullOrWhiteSpace(cols[8]) ? null : decimal.Parse(cols[8]),
        };

        if (mode == "metro")
        {
            stopTime.Timepoint = int.Parse(cols[9]);
            stopTime.StopNote = cols[10];
            stopTime.Mode = "Metro";
        }
        else if (mode == "sydneytrains")
        {
            stopTime.Mode = "Rail";
        }

        return stopTime;
    }
}
