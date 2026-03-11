namespace TransportApi.DTOs;

public class StopDto
{
    public string Id { get; set; } = null!;
    
    public string? Code { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public decimal Latitude { get; set; }
    
    public decimal Longitude { get; set; }
    
    public string? ZoneId { get; set; }
    
    public string? Url { get; set; }
    
    public int LocationType { get; set; }
    
    public string? ParentStationId { get; set; }
    
    public string? Timezone { get; set; }
    
    public int WheelchairBoarding { get; set; }
    
    public int? PlatformCode { get; set; }
    
    public string Mode { get; set; } = null!;

    public string? Network { get; set; }
}