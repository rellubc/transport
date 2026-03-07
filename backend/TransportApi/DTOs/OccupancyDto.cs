using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class OccupancyDto
{
    public string TripId { get; set; } = null!;

    public string StopSequence { get; set; } = null!;

    public int OccupancyStatus { get; set; }

    public bool Monday { get; set; }

    public bool Tuesday { get; set; }

    public bool Wednesday { get; set; }

    public bool Thursday { get; set; }

    public bool Friday { get; set; }

    public bool Saturday { get; set; }

    public bool Sunday { get; set; }

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
    
    public bool? Exception { get; set; }
}