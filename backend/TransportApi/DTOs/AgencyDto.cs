namespace TransportApi.DTOs;

public class AgencyDto
{
    public string Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public string Url { get; set; } = "http://transportnsw.info";
    
    public string Timezone { get; set; } = "Australia/Sydney";
    
    public string Lang { get; set; } = "EN";
    
    public string Phone { get; set; } = "131500";
    
    public string? FareUrl { get; set; } = "http://transportnsw.info";
    
    public string? Email { get; set; } = "information@transport.nsw.gov.au";
}
