namespace TransportApi.DTOs;

public class StopTimeDto
{
    public string TripId { get; set; } = null!;

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan DepartureTime { get; set; }

    public string StopId { get; set; } = null!;

    public string StopName { get; set; } = null!;

    public string RouteId { get; set; } = null!;

    public int StopSequence { get; set; }

    public string? StopHeadSign { get; set; }

    public bool PickupType { get; set; }

    public bool DropOffType { get; set; }

    public decimal ShapeDistanceTravelled { get; set; }

    public bool? Timepoint { get; set; }

    public string? StopNote { get; set; }

    public string Mode { get; set; } = null!;
}