using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportStatic.Models;

[Table("routes")]
public class Route
{
    [Column("route_id")]
    public string Id { get; set; } = null!;

    [Column("agency_id")]
    public string AgencyId { get; set; } = null!;

    [Column("route_short_name")]
    public string? ShortName { get; set; }

    [Column("route_long_name")]
    public string LongName { get; set; } = null!;

    [Column("route_desc")]
    public string Description { get; set; } = null!;

    [Column("route_type")]
    public int Type { get; set; }

    [Column("route_colour")]
    public string Colour { get; set; } = null!;

    [Column("route_text_colour")]
    public string TextColour { get; set; } = null!;

    [Column("route_url")]
    [Url]
    public string? Url { get; set; }
}
