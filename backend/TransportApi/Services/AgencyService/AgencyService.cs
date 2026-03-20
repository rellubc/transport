using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;

namespace TransportApi.Services;

public class AgencyService(TransportDbContext db) : IAgencyService
{
    private readonly TransportDbContext _db = db;

    public async Task<List<AgencyDto>> GetAgencies()
    {
        var agencies = await _db.Agencies
            .Select(a => new AgencyDto
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

    public async Task<AgencyDto?> GetAgency(string agencyId)
    {
        var agency = await _db.Agencies
            .Where(a => a.Id == agencyId)
            .Select(a => new AgencyDto
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