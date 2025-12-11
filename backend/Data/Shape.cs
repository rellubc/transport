using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("shapes")]
    public class Shape
    {
        [Column("shape_id")]
        [Required]
        public string ShapeId { get; set; } = null!;
        
        [Column("shape_pt_lat")]
        [Required]
        public float ShapePtLat { get; set; }
        
        [Column("shape_pt_lon")]
        [Required]
        public float ShapePtLon { get; set; }
        
        [Column("shape_pt_sequence")]
        [Required]
        public int ShapePtSequence { get; set; }
        
        [Column("shape_dist_traveled")]
        public float ShapeDistTraveled { get; set; }
    }
}
