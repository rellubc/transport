using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class ShapeDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Required]
    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    [Required]
    public int Sequence { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal DistanceTravelled { get; set; }
}