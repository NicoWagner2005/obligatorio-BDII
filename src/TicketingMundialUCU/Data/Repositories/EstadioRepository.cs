using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public record PaisSede(string Nombre);

public record Estadio(int IdEstadio, string Nombre, string NombrePaisSede);

public record Sector(int IdEstadio, string IdSector, int CapacidadMax);

public class EstadioRepository(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<PaisSede>> GetAllPaisesSedeAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<PaisSede>(
            """SELECT nombre AS "Nombre" FROM paises_sede ORDER BY nombre""");
    }

    public async Task<IEnumerable<Estadio>> GetAllEstadiosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Estadio>(
            """
            SELECT id_estadio AS "IdEstadio", nombre AS "Nombre", nombre_pais_sede AS "NombrePaisSede"
            FROM estadios
            ORDER BY nombre
            """);
    }

    public async Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(int idEstadio)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Sector>(
            """
            SELECT id_estadio AS "IdEstadio", id_sector AS "IdSector", capacidad_max AS "CapacidadMax"
            FROM sectores
            WHERE id_estadio = @IdEstadio
            ORDER BY id_sector
            """,
            new { IdEstadio = idEstadio });
    }

    public async Task CreatePaisSedeAsync(string nombre)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """INSERT INTO paises_sede (nombre) VALUES (@Nombre)""",
            new { Nombre = nombre });
    }

    public async Task<int> CreateEstadioAsync(
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var idEstadio = await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO estadios (nombre, nombre_pais_sede)
            VALUES (@Nombre, @NombrePaisSede)
            RETURNING id_estadio
            """,
            new { Nombre = nombre, NombrePaisSede = nombrePaisSede },
            transaction);

        foreach (var (idSector, capacidadMax) in sectores)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO sectores (id_estadio, id_sector, capacidad_max)
                VALUES (@IdEstadio, @IdSector, @CapacidadMax)
                """,
                new { IdEstadio = idEstadio, IdSector = idSector, CapacidadMax = capacidadMax },
                transaction);
        }

        await transaction.CommitAsync();
        return idEstadio;
    }

    public async Task UpdateEstadioAsync(
        int idEstadio,
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            """
            UPDATE estadios
            SET nombre = @Nombre, nombre_pais_sede = @NombrePaisSede
            WHERE id_estadio = @IdEstadio
            """,
            new { IdEstadio = idEstadio, Nombre = nombre, NombrePaisSede = nombrePaisSede },
            transaction);

        foreach (var (idSector, capacidadMax) in sectores)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO sectores (id_estadio, id_sector, capacidad_max)
                VALUES (@IdEstadio, @IdSector, @CapacidadMax)
                ON CONFLICT (id_estadio, id_sector)
                DO UPDATE SET capacidad_max = EXCLUDED.capacidad_max
                """,
                new { IdEstadio = idEstadio, IdSector = idSector, CapacidadMax = capacidadMax },
                transaction);
        }

        await transaction.CommitAsync();
    }

    public async Task DeleteEstadioAsync(int idEstadio)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """DELETE FROM estadios WHERE id_estadio = @IdEstadio""",
            new { IdEstadio = idEstadio });
    }
}
