namespace TransportStatic.DTOs;

public class TripDTO
{
    public string TripId { get; set; } = null!;

    public string RouteId { get; set; } = null!;

    public string ServiceId { get; set; } = null!;

    public string ShapeId { get; set; } = null!;

    public string? TripHeadsign { get; set; }

    public int DirectionId { get; set; }

    public string? TripShortName { get; set; }

    public string? BlockId { get; set; }

    public int WheelchairAccessible { get; set; }

    public string? TripNote { get; set; }

    public string? RouteDirection { get; set; }

    public int? BikesAllowed { get; set; }

    public string? VehicleCategoryId { get; set; }
}