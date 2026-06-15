namespace TicketingMundialUCU.Data.Repositories;

public interface IAdministratorJurisdictionRepository
{
    Task<IEnumerable<PaisSede>> GetHostCountriesAsync();
    Task<bool> CountryExistsAsync(string countryName);
    Task<string?> GetCountryForAdministratorAsync(string administratorId);
}
