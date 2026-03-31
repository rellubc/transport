using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportStatic.Models;


[Table("agencies")]
public class Agency
{
    [Key]
    [Column("agency_id")]
    public string Id { get; set; } = null!;
    
    [Column("agency_name")]
    public string Name { get; set; } = null!;
    
    [Column("agency_url")]
    [Url]
    public string Url { get; set; } = null!;
    
    [Column("agency_timezone")]
    public string Timezone { get; set; } = null!;
    
    [Column("agency_language")]
    public string? Language { get; set; }
    
    [Column("agency_phone")]
    [Phone]
    public string? Phone { get; set; } 

    [Column("agency_fare_url")]
    public string? FareUrl { get; set; }

    [Column("agency_email")]
    [EmailAddress]
    public string? Email { get; set; }
}
