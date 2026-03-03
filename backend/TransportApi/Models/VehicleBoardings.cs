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
    [Required]
    public string ChildSequence { get; set; } = null!;
    
    [Column("grandchild_sequence")]
    [Required]
    public string GrandchildSequence { get; set; } = null!;
    
    [Column("boarding_area_id")]
    [Required]
    public string BoardingAreaId { get; set; } = null!;
}
