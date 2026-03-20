namespace TransportApi.DTOs;

public class ShapeDto
{
    public string Id { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Sequence { get; set; }

    public decimal? DistanceTravelled { get; set; }
}

public class ShapeDetails
{
    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Sequence { get; set; }

    public decimal? DistanceTravelled { get; set; }
}