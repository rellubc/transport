using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CalendarDateDto
{
    [Required]
    public int ServiceId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(50)]
    public string ExceptionType { get; set; } = null!;
}