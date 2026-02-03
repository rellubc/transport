using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("routes")]
public class Route
{
    [Column("route_id")]
    [Required]
    public string RouteId { get; set; } = null!;
    
    [Column("agency_id")]
    [Required]
    public string AgencyId { get; set; } = null!;
    
    [Column("route_short_name")]
    [Required]
    public string RouteShortName { get; set; } = null!;
    
    [Column("route_long_name")]
    [Required]
    public string RouteLongName { get; set; } = null!;
    
    [Column("route_desc")]
    [Required]
    public string RouteDesc { get; set; } = null!;
    
    [Column("route_type")]
    [Required]
    public int RouteType { get; set; }
    
    [Column("route_color")]
    [Required]
    public string RouteColor { get; set; } = null!;
    
    [Column("route_text_color")]
    [Required]
    public string RouteTextColor { get; set; } = null!;

    [Column("route_url")]
    [Required]
    public string RouteUrl { get; set; } = null!;
}