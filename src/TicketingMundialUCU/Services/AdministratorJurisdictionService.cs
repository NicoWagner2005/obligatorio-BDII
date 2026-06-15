using Microsoft.AspNetCore.Identity;
using TicketingMundialUCU.Data.Repositories;

namespace TicketingMundialUCU.Services;

public sealed class AdministratorJurisdictionService(
    IAdministratorJurisdictionRepository repository,
    ICurrentUserContext currentUserContext)
{
    public Task<IEnumerable<PaisSede>> GetHostCountriesAsync() =>
        repository.GetHostCountriesAsync();

    public async Task<string> GetCurrentCountryAsync()
    {
        return (await GetCurrentScopeAsync()).CountryName;
    }

    public async Task<AdministratorScope> GetCurrentScopeAsync()
    {
        var administratorId = await currentUserContext.GetRequiredAdministratorIdAsync();
        var country = await repository.GetCountryForAdministratorAsync(administratorId);

        return new AdministratorScope(
            administratorId,
            country ?? throw new InvalidOperationException(
                "El administrador no tiene un país sede asignado."));
    }

    public async Task<IdentityResult> ValidateRegistrationCountryAsync(
        string role,
        string? countryName)
    {
        if (role != UserRoles.Administrador)
        {
            return IdentityResult.Success;
        }

        if (string.IsNullOrWhiteSpace(countryName) ||
            !await repository.CountryExistsAsync(countryName))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidAdministratorCountry",
                Description = "Debe seleccionar un país sede válido para el administrador."
            });
        }

        return IdentityResult.Success;
    }
}

public sealed record AdministratorScope(string AdministratorId, string CountryName);
