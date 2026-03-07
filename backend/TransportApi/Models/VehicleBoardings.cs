using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("vehicle_boardings")]
public class VehicleBoarding
{
    [Column("vehicle_category_id")]
    [Required]
    public string VehicleCategoryId { get; set; } = null!;
    
    [Column("child_sequence")]
    public string? ChildSequence { get; set; }
    
    [Column("grandchild_sequence")]
    public string? GrandchildSequence { get; set; }
    
    [Column("boarding_area_id")]
    [Required]
    public string BoardingAreaId { get; set; } = null!;
}
