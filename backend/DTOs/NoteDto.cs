using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class NoteDto
{
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;

    [Required]
    public string Text { get; set; } = null!;
}