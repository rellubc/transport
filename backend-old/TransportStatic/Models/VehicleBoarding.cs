using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportStatic.Models;

[Table("vehicle_boardings")]
public class VehicleBoarding
{
    [Column("vehicle_category_id")]
    [Required]
    public string VehicleCategoryId { get; set; } = null!;
    
    [Column("child_sequence")]
    public string ChildSequence { get; set; } = null!;
    
    [Column("grandchild_sequence")]
    public int? GrandchildSequence { get; set; }
    
    [Column("boarding_area_id")]
    [Required]
    public string BoardingAreaId { get; set; } = null!;
}
