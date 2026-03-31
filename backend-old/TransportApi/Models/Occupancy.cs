using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TransportStatic.Models;

[Table("occupancies")]
public class Occupancy
{
    [Column("trip_id")]
    public string TripId { get; set; } = null!;

    [Column("stop_sequence")]
    public int StopSequence { get; set; }
    
    [Column("occupancy_status")]
    public int OccupancyStatus { get; set; }

    [Column("monday")]
    public int Monday { get; set; }

    [Column("tuesday")]
    public int Tuesday { get; set; }

    [Column("wednesday")]
    public int Wednesday { get; set; }

    [Column("thursday")]
    public int Thursday { get; set; }

    [Column("friday")]
    public int Friday { get; set; }

    [Column("saturday")]
    public int Saturday { get; set; }

    [Column("sunday")]
    public int Sunday { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("exception")]
    public int? Exception { get; set; }

    public static Occupancy ParseColumns(string[] cols)
    {
        return new Occupancy
        {
            TripId = cols[0],
            StopSequence = int.Parse(cols[1]),
            OccupancyStatus = int.Parse(cols[2]),
            Monday = int.Parse(cols[3]),
            Tuesday = int.Parse(cols[4]),
            Wednesday = int.Parse(cols[5]),
            Thursday = int.Parse(cols[6]),
            Friday = int.Parse(cols[7]),
            Saturday = int.Parse(cols[8]),
            Sunday = int.Parse(cols[9]),
            StartDate = DateOnly.ParseExact(cols[10], "yyyyMMdd", CultureInfo.InvariantCulture),
            EndDate = string.IsNullOrWhiteSpace(cols[11]) ? null : DateOnly.ParseExact(cols[11], "yyyyMMdd", CultureInfo.InvariantCulture),
            Exception = string.IsNullOrWhiteSpace(cols[12]) ? null : int.Parse(cols[12])
        };
    }
}
