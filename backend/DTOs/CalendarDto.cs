using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CalendarDto
{
    [Required]
    public int ServiceId { get; set; }

    [Required]
    public bool Monday { get; set; }

    [Required]
    public bool Tuesday { get; set; }

    [Required]
    public bool Wednesday { get; set; }

    [Required]
    public bool Thursday { get; set; }

    [Required]
    public bool Friday { get; set; }

    [Required]
    public bool Saturday { get; set; }

    [Required]
    public bool Sunday { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
}