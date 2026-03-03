using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models;

[Table("calendar")]
public class Calendar
{
    [Key]
    [Column("service_id")]
    public int ServiceId { get; set; }

    [Column("monday")]
    [Required]
    public bool Monday { get; set; }

    [Column("tuesday")]
    [Required]
    public bool Tuesday { get; set; }

    [Column("wednesday")]
    [Required]
    public bool Wednesday { get; set; }

    [Column("thursday")]
    [Required]
    public bool Thursday { get; set; }

    [Column("friday")]
    [Required]
    public bool Friday { get; set; }

    [Column("saturday")]
    [Required]
    public bool Saturday { get; set; }

    [Column("sunday")]
    [Required]
    public bool Sunday { get; set; }

    [Column("start_date")]
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
}
