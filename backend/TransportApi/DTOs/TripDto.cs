namespace TransportApi.DTOs;

public class TripDto
{
    public string Id { get; set; } = null!;

    public string RouteId { get; set; } = null!;

    public string ServiceId { get; set; } = null!;

    public string ShapeId { get; set; } = null!;

    public string HeadSign { get; set; } = null!;

    public bool DirectionId { get; set; }

    public string ShortName { get; set; } = null!;

    public string? BlockId { get; set; }

    public bool WheelchairAccessible { get; set; }

    public string? TripNote { get; set; }

    public string? RouteDirection { get; set; }

    public bool? BikesAllowed { get; set; }

    public string? VehicleCategoryId { get; set; }
}