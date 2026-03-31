using TransportStatic.DTOs;

namespace TransportStatic.Services;

public interface IAgencyService
{
    Task<List<AgencyDTO>> GetAgencies();
    Task<AgencyDTO?> GetAgency(string agencyId);
}