using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class AgencyDto
{
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    
    [Required]
    [Url]
    [StringLength(255)]
    public string Url { get; set; } = "http://transportnsw.info";
    
    [Required]
    [StringLength(255)]
    public string Timezone { get; set; } = "Australia/Sydney";
    
    [Required]
    [StringLength(255)]
    public string Lang { get; set; } = "EN";
    
    [Required]
    [Phone]
    [StringLength(255)]
    public string Phone { get; set; } = "131500";

    [StringLength(255)]
    public string? FareUrl { get; set; } = "http://transportnsw.info";

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; } = "information@transport.nsw.gov.au";
}
