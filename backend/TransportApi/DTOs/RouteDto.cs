using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class RouteDto
{
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string AgencyId { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string ShortName { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string LongName { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    public int Type { get; set; }

    [Required]
    [StringLength(6)]
    public string Color { get; set; } = "00B5EF";

    [Required]
    [StringLength(6)]
    public string TextColor { get; set; } = "FFFFFF";

    [Required]
    [Url]
    [StringLength(255)]
    public string Url { get; set; } = null!;
}
