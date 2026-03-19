using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("routes")]
public class Route
{
    [Column("route_id")]
    public string Id { get; set; } = null!;

    [Column("agency_id")]
    public string AgencyId { get; set; } = null!;

    [Column("route_short_name")]
    public string ShortName { get; set; } = null!;

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
    public string Url { get; set; } = null!;

    public static Route ParseColumns(string mode, string[] cols)
    {
        var route = new Route
        {
            Id = cols[0],
            AgencyId = cols[1],
            ShortName = cols[2],
            LongName = cols[3],
            Description = cols[4],
            Type = int.Parse(cols[5]),
        };

        if (mode == "metro")
        {
            route.Colour = cols[6];
            route.TextColour = cols[7];
            route.Url = cols[8];
        }
        else if (mode == "sydneytrains")
        {
            route.Url = cols[6];
            route.Colour = cols[7];
            route.TextColour = cols[8];
        }

        return route;
    }
}
