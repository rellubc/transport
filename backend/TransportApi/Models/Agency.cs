using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("agency")]
public class Agency
{
    [Key]
    [Column("agency_id")]
    [Required]
    [StringLength(255)]
    public string Id { get; set; } = null!;
    
    [Column("agency_name")]
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    
    [Column("agency_url")]
    [Required]
    [Url]
    [StringLength(255)]
    public string Url { get; set; } = "http://transportnsw.info";
    
    [Column("agency_timezone")]
    [Required]
    [StringLength(255)]
    public string Timezone { get; set; } = "Australia/Sydney";
    
    [Column("agency_lang")]
    [Required]
    [StringLength(255)]
    public string Lang { get; set; } = "EN";
    
    [Column("agency_phone")]
    [Required]
    [Phone]
    [StringLength(255)]
    public string Phone { get; set; } = "131500";

    [Column("agency_fare_url")]
    [StringLength(255)]
    public string? FareUrl { get; set; } = "http://transportnsw.info";

    [Column("agency_email")]
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; } = "information@transport.nsw.gov.au";
}
