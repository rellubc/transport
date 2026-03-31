using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace TransportStatic.Models;

[Table("shapes")]
public class Shape
{
    [Column("shape_id")]
    public string Id { get; set; } = null!;

    [Column("shape_pt_lat")]
    public decimal Latitude { get; set; }

    [Column("shape_pt_lon")]
    public decimal Longitude { get; set; }

    [Column("geom")]
    public Point Geom { get; set; } = null!;

    [Column("shape_pt_sequence")]
    public int Sequence { get; set; }

    [Column("shape_dist_travelled")]
    public decimal? DistanceTravelled { get; set; }
    
    [Column("mode")]
    public string Mode { get; set; } = null!;
}
