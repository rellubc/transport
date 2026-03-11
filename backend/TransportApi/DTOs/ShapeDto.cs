namespace TransportApi.DTOs;

public class ShapeDetails
{
    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Sequence { get; set; }

    public decimal? DistanceTravelled { get; set; }

    public string Mode { get; set; } = null!;
}