using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("shapes")]
public class Shape
{
    [Column("shape_id")]
    [Required]
    public string Id { get; set; } = null!;

    [Column("shape_pt_lat")]
    [Required]
    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Column("shape_pt_lon")]
    [Required]
    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    [Column("shape_pt_sequence")]
    [Required]
    public int Sequence { get; set; }

    [Column("shape_dist_travelled")]
    [Required]
    [Precision(18, 2)]
    public decimal DistanceTravelled { get; set; }
}
