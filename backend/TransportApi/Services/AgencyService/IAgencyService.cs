using TransportApi.DTOs;

namespace TransportApi.Services;

public interface IAgencyService
{
    Task<List<AgencyDto>> GetAgencies();
    Task<AgencyDto?> GetAgency(string agencyId);
}