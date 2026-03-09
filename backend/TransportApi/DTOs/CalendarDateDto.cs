namespace TransportApi.DTOs;

public class CalendarDateDto
{
    public int ServiceId { get; set; }

    public DateTime Date { get; set; }

    public string ExceptionType { get; set; } = null!;
}