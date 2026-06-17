namespace TicketingMundialUCU.Data.Daos;

public interface IAdministratorJurisdictionDao
{
    Task<IEnumerable<PaisSede>> GetHostCountriesAsync();
    Task<bool> CountryExistsAsync(string countryName);
    Task<string?> GetCountryForAdministratorAsync(string administratorId);
}
