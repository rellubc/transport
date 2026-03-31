namespace TransportStatic.DTOs;

public class RouteDTO
{
    public string Id { get; set; } = null!;
    
    public string AgencyId { get; set; } = null!;
    
    public string ShortName { get; set; } = null!;
    
    public string LongName { get; set; } = null!;
    
    public string Description { get; set; } = null!;
    
    public int Type { get; set; }
    
    public string Colour { get; set; } = null!;
    
    public string TextColour { get; set; } = null!;
    
    public string Url { get; set; } = null!;
}
