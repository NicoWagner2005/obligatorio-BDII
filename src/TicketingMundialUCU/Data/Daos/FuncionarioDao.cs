using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Daos;

public class FuncionarioDao(IConfiguration configuration) : IFuncionarioDao
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<IEnumerable<FuncionarioInfo>> GetAllFuncionariosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<FuncionarioInfo>(
            """
            SELECT
                f.usuario_id AS "UsuarioId",
                f.nro_legajo AS "NroLegajo",
                u."Email"    AS "Email"
            FROM funcionarios f
            JOIN "AspNetUsers" u ON u."Id" = f.usuario_id
            ORDER BY f.nro_legajo
            """);
    }

    public async Task<IEnumerable<DispositivoAutorizado>> GetAllDispositivosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<DispositivoAutorizado>(
            """
            SELECT
                d.id_dispositivo  AS "IdDispositivo",
                d.identificador   AS "Identificador",
                d.id_funcionario  AS "IdFuncionario",
                f.nro_legajo      AS "NroLegajo",
                d.fecha_registro  AS "FechaRegistro",
                EXISTS (
                    SELECT 1 FROM validaciones_acceso va
                    WHERE va.id_dispositivo = d.id_dispositivo
                ) AS "TieneValidaciones"
            FROM dispositivos_autorizados d
            JOIN funcionarios f ON f.usuario_id = d.id_funcionario
            ORDER BY f.nro_legajo, d.identificador
            """);
    }

    public async Task<IEnumerable<DispositivoAutorizado>> GetDispositivosByFuncionarioAsync(string idFuncionario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<DispositivoAutorizado>(
            """
            SELECT
                d.id_dispositivo  AS "IdDispositivo",
                d.identificador   AS "Identificador",
                d.id_funcionario  AS "IdFuncionario",
                f.nro_legajo      AS "NroLegajo",
                d.fecha_registro  AS "FechaRegistro",
                EXISTS (
                    SELECT 1 FROM validaciones_acceso va
                    WHERE va.id_dispositivo = d.id_dispositivo
                ) AS "TieneValidaciones"
            FROM dispositivos_autorizados d
            JOIN funcionarios f ON f.usuario_id = d.id_funcionario
            WHERE d.id_funcionario = @IdFuncionario
            ORDER BY d.identificador
            """,
            new { IdFuncionario = idFuncionario });
    }

    public async Task<bool> IsDispositivoDelFuncionarioAsync(int idDispositivo, string idFuncionario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1 FROM dispositivos_autorizados
                WHERE id_dispositivo = @IdDispositivo
                  AND id_funcionario = @IdFuncionario
            )
            """,
            new { IdDispositivo = idDispositivo, IdFuncionario = idFuncionario });
    }

    public async Task CreateDispositivoAsync(string identificador, string idFuncionario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO dispositivos_autorizados (identificador, id_funcionario)
            VALUES (@Identificador, @IdFuncionario)
            """,
            new { Identificador = identificador, IdFuncionario = idFuncionario });
    }

    public async Task<bool> DeleteDispositivoAsync(int idDispositivo)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var deleted = await connection.ExecuteAsync(
            """DELETE FROM dispositivos_autorizados WHERE id_dispositivo = @IdDispositivo""",
            new { IdDispositivo = idDispositivo });
        return deleted == 1;
    }

    public async Task<IEnumerable<AsignacionSector>> GetAllAsignacionesAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<AsignacionSector>(
            """
            SELECT
                fse.id_funcionario  AS "IdFuncionario",
                f.nro_legajo        AS "NroLegajo",
                fse.id_evento       AS "IdEvento",
                fse.id_estadio      AS "IdEstadio",
                fse.id_sector       AS "IdSector",
                est.nombre          AS "NombreEstadio",
                e.fecha_hora        AS "FechaHora",
                eq_l.nombre         AS "EquipoLocal",
                eq_v.nombre         AS "EquipoVisitante"
            FROM funcionario_sector_evento fse
            JOIN funcionarios f      ON f.usuario_id   = fse.id_funcionario
            JOIN eventos e           ON e.id_evento    = fse.id_evento
            JOIN estadios est        ON est.id_estadio = fse.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            ORDER BY f.nro_legajo, e.fecha_hora, fse.id_sector
            """);
    }

    public async Task<IEnumerable<AsignacionSector>> GetAsignacionesByEventoAsync(int idEvento)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<AsignacionSector>(
            """
            SELECT
                fse.id_funcionario  AS "IdFuncionario",
                f.nro_legajo        AS "NroLegajo",
                fse.id_evento       AS "IdEvento",
                fse.id_estadio      AS "IdEstadio",
                fse.id_sector       AS "IdSector",
                est.nombre          AS "NombreEstadio",
                e.fecha_hora        AS "FechaHora",
                eq_l.nombre         AS "EquipoLocal",
                eq_v.nombre         AS "EquipoVisitante"
            FROM funcionario_sector_evento fse
            JOIN funcionarios f      ON f.usuario_id   = fse.id_funcionario
            JOIN eventos e           ON e.id_evento    = fse.id_evento
            JOIN estadios est        ON est.id_estadio = fse.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            WHERE fse.id_evento = @IdEvento
            ORDER BY f.nro_legajo, fse.id_sector
            """,
            new { IdEvento = idEvento });
    }

    public async Task<IEnumerable<AsignacionSector>> GetAsignacionesByFuncionarioAsync(string idFuncionario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<AsignacionSector>(
            """
            SELECT
                fse.id_funcionario  AS "IdFuncionario",
                f.nro_legajo        AS "NroLegajo",
                fse.id_evento       AS "IdEvento",
                fse.id_estadio      AS "IdEstadio",
                fse.id_sector       AS "IdSector",
                est.nombre          AS "NombreEstadio",
                e.fecha_hora        AS "FechaHora",
                eq_l.nombre         AS "EquipoLocal",
                eq_v.nombre         AS "EquipoVisitante"
            FROM funcionario_sector_evento fse
            JOIN funcionarios f      ON f.usuario_id   = fse.id_funcionario
            JOIN eventos e           ON e.id_evento    = fse.id_evento
            JOIN estadios est        ON est.id_estadio = fse.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            WHERE fse.id_funcionario = @IdFuncionario
            ORDER BY e.fecha_hora DESC, fse.id_sector
            """,
            new { IdFuncionario = idFuncionario });
    }

    public async Task<bool> ExisteAsignacionAsync(
        string idFuncionario, int idEvento, int idEstadio, string idSector)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1 FROM funcionario_sector_evento
                WHERE id_funcionario = @IdFuncionario
                  AND id_evento      = @IdEvento
                  AND id_estadio     = @IdEstadio
                  AND id_sector      = @IdSector
            )
            """,
            new { IdFuncionario = idFuncionario, IdEvento = idEvento, IdEstadio = idEstadio, IdSector = idSector });
    }

    public async Task CreateAsignacionAsync(
        string idFuncionario, int idEvento, int idEstadio, string idSector)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO funcionario_sector_evento (id_funcionario, id_evento, id_estadio, id_sector)
            VALUES (@IdFuncionario, @IdEvento, @IdEstadio, @IdSector)
            """,
            new { IdFuncionario = idFuncionario, IdEvento = idEvento, IdEstadio = idEstadio, IdSector = idSector });
    }

    public async Task<bool> DeleteAsignacionAsync(
        string idFuncionario, int idEvento, int idEstadio, string idSector)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var deleted = await connection.ExecuteAsync(
            """
            DELETE FROM funcionario_sector_evento
            WHERE id_funcionario = @IdFuncionario
              AND id_evento      = @IdEvento
              AND id_estadio     = @IdEstadio
              AND id_sector      = @IdSector
            """,
            new { IdFuncionario = idFuncionario, IdEvento = idEvento, IdEstadio = idEstadio, IdSector = idSector });
        return deleted == 1;
    }

    public async Task<EntradaValidacionInfo?> GetEntradaParaValidarAsync(Guid idEntrada)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<EntradaValidacionInfo>(
            """
            SELECT
                en.id_entrada              AS "IdEntrada",
                EXISTS (
                    SELECT 1
                    FROM validaciones_acceso va
                    WHERE va.id_entrada = en.id_entrada
                )                          AS "Consumida",
                dv.id_evento               AS "IdEvento",
                dv.id_estadio              AS "IdEstadio",
                dv.id_sector               AS "IdSector"
            FROM entradas en
            JOIN detalle_venta dv
                ON en.id_venta = dv.id_venta
               AND en.nro_linea_detalle_venta = dv.nro_linea
            WHERE en.id_entrada = @IdEntrada
            """,
            new { IdEntrada = idEntrada });
    }

    public async Task ValidarEntradaAsync(Guid idEntrada, string idFuncionario, int idDispositivo)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var entradaBloqueada = await connection.QuerySingleOrDefaultAsync<Guid?>(
            """
            SELECT id_entrada
            FROM entradas
            WHERE id_entrada = @IdEntrada
            FOR UPDATE
            """,
            new { IdEntrada = idEntrada },
            transaction);

        if (entradaBloqueada is null)
            throw new InvalidOperationException("No se encontró la entrada.");

        var yaValidada = await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM validaciones_acceso
                WHERE id_entrada = @IdEntrada
            )
            """,
            new { IdEntrada = idEntrada },
            transaction);

        if (yaValidada)
            throw new InvalidOperationException("La entrada ya fue validada anteriormente.");

        await connection.ExecuteAsync(
            """
            INSERT INTO validaciones_acceso (id_entrada, id_funcionario, id_dispositivo)
            VALUES (@IdEntrada, @IdFuncionario, @IdDispositivo)
            """,
            new { IdEntrada = idEntrada, IdFuncionario = idFuncionario, IdDispositivo = idDispositivo },
            transaction);

        await transaction.CommitAsync();
    }

    public async Task<IEnumerable<CoberturasSector>> GetCoberturaSectoresAsync(
        string idFuncionario, int idEvento)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<CoberturasSector>(
            """
            SELECT
                fse.id_sector AS "IdSector",
                EXISTS (
                    SELECT 1
                    FROM validaciones_acceso va
                    JOIN entradas en ON en.id_entrada = va.id_entrada
                    JOIN detalle_venta dv
                        ON dv.id_venta = en.id_venta
                       AND dv.nro_linea = en.nro_linea_detalle_venta
                    WHERE va.id_funcionario = fse.id_funcionario
                      AND dv.id_evento      = fse.id_evento
                      AND dv.id_sector      = fse.id_sector
                ) AS "Validado"
            FROM funcionario_sector_evento fse
            WHERE fse.id_funcionario = @IdFuncionario
              AND fse.id_evento      = @IdEvento
            ORDER BY fse.id_sector
            """,
            new { IdFuncionario = idFuncionario, IdEvento = idEvento });
    }

}
