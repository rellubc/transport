namespace TransportApi.DTOs;

public class VehicleBoarding
{
    public string VehicleCategoryId { get; set; } = null!;
    
    public string? ChildSequence { get; set; }
    
    public string? GrandchildSequence { get; set; }
    
    public string BoardingAreaId { get; set; } = null!;
}
