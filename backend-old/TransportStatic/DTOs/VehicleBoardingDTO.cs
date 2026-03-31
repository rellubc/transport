namespace TransportStatic.DTOs;

public class VehicleBoardingDTO
{
    public string VehicleCategoryId { get; set; } = null!;
    
    public string ChildSequence { get; set; } = null!;
    
    public string GrandchildSequence { get; set; } = null!;
    
    public string BoardingAreaId { get; set; } = null!;
}
