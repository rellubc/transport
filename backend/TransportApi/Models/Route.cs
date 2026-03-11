using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("routes")]
public class Route
{
    [Column("route_id")]
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;

    [Column("agency_id")]
    [Required]
    [StringLength(255)]
    public string AgencyId { get; set; } = null!;

    [Column("route_short_name")]
    [Required]
    [StringLength(255)]
    public string ShortName { get; set; } = null!;

    [Column("route_long_name")]
    [Required]
    [StringLength(255)]
    public string LongName { get; set; } = null!;

    [Column("route_desc")]
    [Required]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    [Column("route_type")]
    [Required]
    public int Type { get; set; }

    [Column("route_colour")]
    [Required]
    [StringLength(6)]
    public string Colour { get; set; } = "00B5EF";

    [Column("route_text_colour")]
    [Required]
    [StringLength(6)]
    public string TextColour { get; set; } = "FFFFFF";

    [Column("route_url")]
    [Required]
    [Url]
    [StringLength(255)]
    public string Url { get; set; } = null!;
}
