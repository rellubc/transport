namespace TransportApi.DTOs;

public class RouteDto
{
    public string Id { get; set; } = null!;
    
    public string AgencyId { get; set; } = null!;
    
    public string ShortName { get; set; } = null!;
    
    public string LongName { get; set; } = null!;
    
    public string Description { get; set; } = null!;
    
    public int Type { get; set; }
    
    public string Colour { get; set; } = "00B5EF";
    
    public string TextColour { get; set; } = "FFFFFF";
    
    public string Url { get; set; } = null!;
}
