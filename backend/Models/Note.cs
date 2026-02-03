using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("notes")]
public class Note
{
    [Column("note_id")]
    [Required]
    public string NoteId { get; set; } = null!;
    
    [Column("note_text")]
    [Required]
    public string NoteText { get; set; } = null!;
}