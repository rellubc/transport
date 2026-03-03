using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class CalendarDateDto
{
    public int ServiceId { get; set; }

    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [StringLength(50)]
    public string ExceptionType { get; set; } = null!;
}