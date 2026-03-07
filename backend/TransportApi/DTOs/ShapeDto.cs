using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TransportApi.DTOs;

public class ShapeDto
{
    public string Id { get; set; } = null!;

    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    public int Sequence { get; set; }

    [Precision(18, 2)]
    public decimal DistanceTravelled { get; set; }
}

public class ShapeDetails
{
    [Precision(11, 8)]
    public decimal Latitude { get; set; }

    [Precision(11, 8)]
    public decimal Longitude { get; set; }

    public int Sequence { get; set; }

    [Precision(18, 2)]
    public decimal DistanceTravelled { get; set; }
}