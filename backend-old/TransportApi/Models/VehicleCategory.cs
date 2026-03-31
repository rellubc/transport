using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportStatic.Models;

[Table("vehicle_categories")]
public class VehicleCategory
{
    [Column("vehicle_category_id")]
    [Required]
    public string VehicleCategoryId { get; set; } = null!;
    
    [Column("vehicle_category_name")]
    public string VehicleCategoryName { get; set; } = null!;

    public static VehicleCategory ParseColumns(string[] cols)
    {
        return new VehicleCategory
        {
            VehicleCategoryId = cols[0],
            VehicleCategoryName = cols[1]
        };
    }
}
