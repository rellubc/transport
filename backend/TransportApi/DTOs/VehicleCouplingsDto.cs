namespace TransportApi.DTOs;

public class VehicleCouplingDto
{
    public string ParentId { get; set; } = null!;
    
    public string ChildId { get; set; } = null!;
    
    public int ChildSequence { get; set; }
    
    public string? ChildLabel { get; set; }
}
