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
    public decimal? DistanceTravelled { get; set; }
    
    [Column("mode")]
    public string Mode { get; set; } = null!;

    public static Shape ParseColumns(string mode, string[] cols)
    {
        var shape = new Shape
        {
            Latitude = decimal.Parse(cols[1]),
            Longitude = decimal.Parse(cols[2]),
            Sequence = int.Parse(cols[3]),
            DistanceTravelled = string.IsNullOrWhiteSpace(cols[4]) ? null : decimal.Parse(cols[4]),
        };

        if (mode == "metro")
        {
            shape.Id = "M1_" +cols[0];
            shape.Mode = "Metro";
        }
        else if (mode == "sydneytrains")
        {
            shape.Id = cols[0];
            shape.Mode = "Rail";
        }

        return shape;
    }
}
