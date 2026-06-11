using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public record Equipo(int IdEquipo, string Nombre);

public record SectorHabilitado(string IdSector, decimal Precio);

public record EventoDetalle(
    int IdEvento,
    DateTime FechaHora,
    int IdEstadio,
    string NombreEstadio,
    string? EquipoLocal,
    string? EquipoVisitante);

public class EventoRepository(IConfiguration configuration) : IEventoRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<Equipo>> GetAllEquiposAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Equipo>(
            """SELECT id_equipo AS "IdEquipo", nombre AS "Nombre" FROM equipos ORDER BY nombre""");
    }

    public async Task CreateEquipoAsync(string nombre)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """INSERT INTO equipos (nombre) VALUES (@Nombre)""",
            new { Nombre = nombre });
    }

    public async Task<IEnumerable<EventoDetalle>> GetAllEventosDetalladosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<EventoDetalle>(
            """
            SELECT
                e.id_evento          AS "IdEvento",
                e.fecha_hora         AS "FechaHora",
                e.id_estadio         AS "IdEstadio",
                est.nombre           AS "NombreEstadio",
                eq_l.nombre          AS "EquipoLocal",
                eq_v.nombre          AS "EquipoVisitante"
            FROM eventos e
            JOIN estadios est ON e.id_estadio = est.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            ORDER BY e.fecha_hora DESC
            """);
    }

    public async Task<Dictionary<int, List<SectorHabilitado>>> GetAllSectoresHabilitadosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var rows = await connection.QueryAsync<(int IdEvento, string IdSector, decimal Precio)>(
            """
            SELECT id_evento AS "IdEvento", id_sector AS "IdSector", precio AS "Precio"
            FROM evento_habilita_sector
            ORDER BY id_evento, id_sector
            """);
        return rows
            .GroupBy(r => r.IdEvento)
            .ToDictionary(g => g.Key, g => g.Select(r => new SectorHabilitado(r.IdSector, r.Precio)).ToList());
    }

    // RF-31: verifica que no haya otro evento en el mismo estadio dentro de una
    // ventana de 3 horas (duración asumida de un partido).
    public async Task<bool> ExisteSuperposicionAsync(
        int idEstadio,
        DateTime fechaHora,
        int? idEventoExcluir = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM eventos
            WHERE id_estadio = @IdEstadio
              AND (@IdEventoExcluir IS NULL OR id_evento != @IdEventoExcluir)
              AND fecha_hora > @FechaHora - INTERVAL '3 hours'
              AND fecha_hora < @FechaHora + INTERVAL '3 hours'
            """,
            new
            {
                IdEstadio = idEstadio,
                FechaHora = DateTime.SpecifyKind(fechaHora, DateTimeKind.Unspecified),
                IdEventoExcluir = idEventoExcluir
            });
        return count > 0;
    }

    public async Task<int> CreateEventoAsync(
        DateTime fechaHora,
        string idAdministrador,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var idEvento = await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO eventos (fecha_hora, id_administrador, id_estadio)
            VALUES (@FechaHora, @IdAdministrador, @IdEstadio)
            RETURNING id_evento
            """,
            new
            {
                FechaHora = DateTime.SpecifyKind(fechaHora, DateTimeKind.Unspecified),
                IdAdministrador = idAdministrador,
                IdEstadio = idEstadio
            },
            transaction);

        await connection.ExecuteAsync(
            """
            INSERT INTO equipo_juega_evento (id_equipo, id_evento, rol)
            VALUES (@IdEquipo, @IdEvento, @Rol)
            """,
            new[]
            {
                new { IdEquipo = idEquipoLocal,    IdEvento = idEvento, Rol = "local"     },
                new { IdEquipo = idEquipoVisitante, IdEvento = idEvento, Rol = "visitante" }
            },
            transaction);

        foreach (var (sector, precio) in sectoresConPrecio)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO evento_habilita_sector (id_evento, id_estadio, id_sector, precio)
                VALUES (@IdEvento, @IdEstadio, @IdSector, @Precio)
                """,
                new { IdEvento = idEvento, IdEstadio = idEstadio, IdSector = sector, Precio = precio },
                transaction);
        }

        await transaction.CommitAsync();
        return idEvento;
    }

    public async Task UpdateEventoAsync(
        int idEvento,
        DateTime fechaHora,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            """
            UPDATE eventos
            SET fecha_hora = @FechaHora, id_estadio = @IdEstadio
            WHERE id_evento = @IdEvento
            """,
            new
            {
                IdEvento = idEvento,
                FechaHora = DateTime.SpecifyKind(fechaHora, DateTimeKind.Unspecified),
                IdEstadio = idEstadio
            },
            transaction);

        await connection.ExecuteAsync(
            """DELETE FROM equipo_juega_evento WHERE id_evento = @IdEvento""",
            new { IdEvento = idEvento }, transaction);

        await connection.ExecuteAsync(
            """
            INSERT INTO equipo_juega_evento (id_equipo, id_evento, rol)
            VALUES (@IdEquipo, @IdEvento, @Rol)
            """,
            new[]
            {
                new { IdEquipo = idEquipoLocal,    IdEvento = idEvento, Rol = "local"     },
                new { IdEquipo = idEquipoVisitante, IdEvento = idEvento, Rol = "visitante" }
            },
            transaction);

        await connection.ExecuteAsync(
            """DELETE FROM evento_habilita_sector WHERE id_evento = @IdEvento""",
            new { IdEvento = idEvento }, transaction);

        foreach (var (sector, precio) in sectoresConPrecio)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO evento_habilita_sector (id_evento, id_estadio, id_sector, precio)
                VALUES (@IdEvento, @IdEstadio, @IdSector, @Precio)
                """,
                new { IdEvento = idEvento, IdEstadio = idEstadio, IdSector = sector, Precio = precio },
                transaction);
        }

        await transaction.CommitAsync();
    }

    public async Task DeleteEventoAsync(int idEvento)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """DELETE FROM eventos WHERE id_evento = @IdEvento""",
            new { IdEvento = idEvento });
    }
}
