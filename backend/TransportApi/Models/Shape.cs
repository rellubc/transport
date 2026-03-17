using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("shapes")]
public class Shape
{
    [Column("shape_id")]
    public string Id { get; set; } = null!;

    [Column("shape_pt_lat")]
    public decimal Latitude { get; set; }

    [Column("shape_pt_lon")]
    public decimal Longitude { get; set; }

    [Column("shape_pt_sequence")]
    public int Sequence { get; set; }

    [Column("shape_dist_travelled")]
    public decimal DistanceTravelled { get; set; }

    public static Shape ParseColumns(string[] cols)
    {
        return new Shape
        {
            Id = cols[0],
            Latitude = decimal.Parse(cols[1]),
            Longitude = decimal.Parse(cols[2]),
            Sequence = int.Parse(cols[3]),
            DistanceTravelled = decimal.Parse(cols[4])
        };
    }
}
