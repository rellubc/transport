using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("calendar_dates")]
public class CalendarDate
{
    [Key]
    [Column("service_id")]
    public int ServiceId { get; set; }

    [Column("date")]
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Column("exception_type")]
    [Required]
    [StringLength(50)]
    public string ExceptionType { get; set; } = null!;
}
