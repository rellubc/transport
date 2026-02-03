using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("vehicle_categories")]
public class VehicleCategory
{
    [Column("vehicle_category_id")]
    [Required]
    public string VehicleCategoryId { get; set; } = null!;
    
    [Column("vehicle_category_name")]
    [Required]
    public string VehicleCategoryName { get; set; } = null!;
}
