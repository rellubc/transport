using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TransportApi.Models;

[Table("calendar")]
public class Calendar
{
    [Key]
    [Column("service_id")]
    public string ServiceId { get; set; } = null!;

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
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    public static Calendar ParseColumns(string[] cols)
    {
        return new Calendar
        {
            ServiceId = cols[0],
            Monday = int.Parse(cols[1]),
            Tuesday = int.Parse(cols[2]),
            Wednesday = int.Parse(cols[3]),
            Thursday = int.Parse(cols[4]),
            Friday = int.Parse(cols[5]),
            Saturday = int.Parse(cols[6]),
            Sunday = int.Parse(cols[7]),
            StartDate = DateTime.ParseExact(cols[8], "yyyyMMdd", CultureInfo.InvariantCulture),
            EndDate = DateTime.ParseExact(cols[9], "yyyyMMdd", CultureInfo.InvariantCulture)
        };
    }
}
