using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class StopStationDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Precision(11, 8)]
    public decimal Longitude { get; set; }
    public string LocationType { get; set; } = null!;
}