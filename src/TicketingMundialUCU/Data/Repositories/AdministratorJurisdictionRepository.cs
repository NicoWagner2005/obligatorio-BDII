using Dapper;
using Npgsql;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Data.Repositories;

public sealed class AdministratorJurisdictionRepository(IConfiguration configuration)
    : IAdministratorJurisdictionRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<PaisSede>> GetHostCountriesAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<PaisSede>(
            """
            SELECT nombre AS "Nombre"
            FROM paises_sede
            WHERE nombre = ANY(@CountryNames)
            ORDER BY nombre
            """,
            new { CountryNames = HostCountries.All });
    }

    public async Task<bool> CountryExistsAsync(string countryName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM paises_sede
                WHERE nombre = @CountryName
                  AND nombre = ANY(@CountryNames)
            )
            """,
            new { CountryName = countryName, CountryNames = HostCountries.All });
    }

    public async Task<string?> GetCountryForAdministratorAsync(string administratorId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<string>(
            """
            SELECT nombre_pais_sede
            FROM administrador_asignado_a_pais_sede
            WHERE id_administrador = @AdministratorId
            """,
            new { AdministratorId = administratorId });
    }
}
