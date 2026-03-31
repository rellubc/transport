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
    public string Language { get; set; } = null!;
    
    [Column("agency_phone")]
    [Phone]
    public string Phone { get; set; } = null!; 

    [Column("agency_fare_url")]
    public string FareUrl { get; set; } = null!;

    [Column("agency_email")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public static Agency ParseColumns(string mode, string[] cols)
    {
        var agency = new Agency
        {
            Id = cols[0],
            Name = cols[1],
            Url = cols[2],
            Timezone = cols[3],
            Language = cols[4],
            Phone = cols[5],
        };

        if (mode == "metro")
        {
            agency.FareUrl = cols[6];
            agency.Email = cols[7];
        }
        return agency;
    }
}
