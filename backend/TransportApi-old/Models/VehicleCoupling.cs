using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("vehicle_couplings")]
public class VehicleCoupling
{
    [Column("parent_id")]
    [Required]
    public string ParentId { get; set; } = null!;
    
    [Column("child_id")]
    [Required]
    public string ChildId { get; set; } = null!;
    
    [Column("child_sequence")]
    [Required]
    public int ChildSequence { get; set; }
    
    [Column("child_label")]
    public string ChildLabel { get; set; } = null!;

    public static VehicleCoupling ParseColumns(string[] cols)
    {
        return new VehicleCoupling
        {
            ParentId = cols[0],
            ChildId = cols[1],
            ChildSequence = int.Parse(cols[2]),
            ChildLabel = cols[3]
        };
    }
}
