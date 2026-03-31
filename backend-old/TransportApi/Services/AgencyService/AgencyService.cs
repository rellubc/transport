using Microsoft.EntityFrameworkCore;

using TransportStatic.Data;
using TransportStatic.DTOs;

namespace TransportStatic.Services;

public class AgencyService(TransportDbContext db) : IAgencyService
{
    private readonly TransportDbContext _db = db;

    public async Task<List<AgencyDTO>> GetAgencies()
    {
        var agencies = await _db.Agencies
            .Select(a => new AgencyDTO
            {
                Id = a.Id,
                Name = a.Name,
                Url = a.Url,
                Timezone = a.Timezone,
                Language = a.Language,
                Phone = a.Phone,
                FareUrl = a.FareUrl,
                Email = a.Email
            })
            .ToListAsync();

        return agencies;
    }

    public async Task<AgencyDTO?> GetAgency(string agencyId)
    {
        var agency = await _db.Agencies
            .Where(a => a.Id == agencyId)
            .Select(a => new AgencyDTO
            {
                Id = a.Id,
                Name = a.Name,
                Url = a.Url,
                Timezone = a.Timezone,
                Language = a.Language,
                Phone = a.Phone,
                FareUrl = a.FareUrl,
                Email = a.Email
            })
            .FirstOrDefaultAsync();

        return agency;
    }
}