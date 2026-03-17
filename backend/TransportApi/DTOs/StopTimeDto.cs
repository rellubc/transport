namespace TransportApi.DTOs;

public class StopTimeDto
{
    public string TripId { get; set; } = null!;

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan DepartureTime { get; set; }

    public string? StopName { get; set; }

    public string StopId { get; set; } = null!;
    
    public string? RouteId { get; set; }

    public int StopSequence { get; set; }

    public string StopHeadSign { get; set; } = null!;

    public int PickupType { get; set; }

    public int DropOffType { get; set; }

    public decimal? ShapeDistanceTravelled { get; set; }

    public int? Timepoint { get; set; }

    public string? StopNote { get; set; }
}