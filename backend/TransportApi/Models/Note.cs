using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("notes")]
public class Note
{
    [Key]
    [Column("note_id")]
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;

    [Column("note_text")]
    [Required]
    public string Text { get; set; } = null!;
}
