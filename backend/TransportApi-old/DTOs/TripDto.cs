namespace TransportApi.DTOs;

public class TripDto
{
    public string Id { get; set; } = null!;

    public string RouteId { get; set; } = null!;

    public string ServiceId { get; set; } = null!;

    public string ShapeId { get; set; } = null!;

    public string HeadSign { get; set; } = null!;

    public int DirectionId { get; set; }

    public string ShortName { get; set; } = null!;

    public string BlockId { get; set; } = null!;

    public int WheelchairAccessible { get; set; }

    public string? TripNote { get; set; }

    public string? RouteDirection { get; set; }

    public int? BikesAllowed { get; set; }

    public string? VehicleCategoryId { get; set; }
}