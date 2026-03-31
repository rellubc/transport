namespace TransportApi.DTOs;

public class AgencyDto
{
    public string Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public string Url { get; set; } = null!;
    
    public string Timezone { get; set; } = null!;
    
    public string? Language { get; set; }
    
    public string? Phone { get; set; }
    
    public string? FareUrl { get; set; }
    
    public string? Email { get; set; }
}
