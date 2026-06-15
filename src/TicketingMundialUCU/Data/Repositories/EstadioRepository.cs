using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public record PaisSede(string Nombre);

public record Estadio(int IdEstadio, string Nombre, string NombrePaisSede);

public record Sector(int IdEstadio, string IdSector, int CapacidadMax);

public class EstadioRepository(IConfiguration configuration) : IEstadioRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<Estadio>> GetAllEstadiosAsync(string nombrePaisSede)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Estadio>(
            """
            SELECT id_estadio AS "IdEstadio", nombre AS "Nombre", nombre_pais_sede AS "NombrePaisSede"
            FROM estadios
            WHERE nombre_pais_sede = @NombrePaisSede
            ORDER BY nombre
            """,
            new { NombrePaisSede = nombrePaisSede });
    }

    public async Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(
        int idEstadio,
        string nombrePaisSede)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Sector>(
            """
            SELECT
                s.id_estadio AS "IdEstadio",
                s.id_sector AS "IdSector",
                s.capacidad_max AS "CapacidadMax"
            FROM sectores s
            JOIN estadios e ON e.id_estadio = s.id_estadio
            WHERE s.id_estadio = @IdEstadio
              AND e.nombre_pais_sede = @NombrePaisSede
            ORDER BY s.id_sector
            """,
            new { IdEstadio = idEstadio, NombrePaisSede = nombrePaisSede });
    }

    public async Task<bool> BelongsToCountryAsync(int idEstadio, string nombrePaisSede)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM estadios
                WHERE id_estadio = @IdEstadio
                  AND nombre_pais_sede = @NombrePaisSede
            )
            """,
            new { IdEstadio = idEstadio, NombrePaisSede = nombrePaisSede });
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

    public async Task<bool> UpdateEstadioAsync(
        int idEstadio,
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var updated = await connection.ExecuteAsync(
            """
            UPDATE estadios
            SET nombre = @Nombre
            WHERE id_estadio = @IdEstadio
              AND nombre_pais_sede = @NombrePaisSede
            """,
            new { IdEstadio = idEstadio, Nombre = nombre, NombrePaisSede = nombrePaisSede },
            transaction);

        if (updated == 0)
        {
            await transaction.RollbackAsync();
            return false;
        }

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
        return true;
    }

    public async Task<bool> DeleteEstadioAsync(int idEstadio, string nombrePaisSede)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var deleted = await connection.ExecuteAsync(
            """
            DELETE FROM estadios
            WHERE id_estadio = @IdEstadio
              AND nombre_pais_sede = @NombrePaisSede
            """,
            new { IdEstadio = idEstadio, NombrePaisSede = nombrePaisSede });
        return deleted == 1;
    }
}
