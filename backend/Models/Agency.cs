using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("agency")]
public class Agency
{
    [Column("agency_id")]
    [Required]
    public string AgencyId { get; set; } = null!;
    
    [Column("agency_name")]
    [Required]
    public string AgencyName { get; set; } = null!;
    
    [Column("agency_url")]
    [Required]
    public string AgencyUrl { get; set; } = "http://transportnsw.info";
    
    [Column("agency_timezone")]
    [Required]
    public string AgencyTimezone { get; set; } = "Australia/Sydney";
    
    [Column("agency_lang")]
    [Required]
    public string AgencyLang { get; set; } = "EN";
    
    [Column("agency_phone")]
    [Required]
    public string AgencyPhone { get; set; } = "131500";

    [Column("agency_fare_url")]
    public string? AgencyFareUrl { get; set; } = "http://transportnsw.info";

    [Column("agency_email")]
    public string? AgencyEmail { get; set; } = "information@transport.nsw.gov.au";
}
