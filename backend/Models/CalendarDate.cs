using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("calendar_dates")]
public class CalendarDate
{
    [Column("service_id")]
    [Required]
    public int ServiceId { get; set; }

    [Column("date")]
    [Required]
    public DateOnly Date { get; set; }

    [Column("exception_type")]
    [Required]
    public string ExceptionType { get; set; } = null!;
}
