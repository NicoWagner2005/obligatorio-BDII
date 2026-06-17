using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Daos;

public record TransferenciaDetalle(
    int IdTransferencia,
    Guid IdEntrada,
    string IdSolicitante,
    string EmailSolicitante,
    string IdReceptor,
    string EmailReceptor,
    string Estado,
    DateTime FechaSolicitud,
    int IdVenta,
    int NroLineaDetalleVenta,
    int IdEvento,
    DateTime FechaHoraEvento,
    string NombreEstadio,
    string? EquipoLocal,
    string? EquipoVisitante,
    string IdSector,
    decimal PrecioUnitario,
    string EstadoVenta);

public class TransferenciaDao(IConfiguration configuration) : ITransferenciaDao
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    private const string TransferenciaDetalleSelectFrom = """
        SELECT
            t.id_transferencia                AS "IdTransferencia",
            t.id_entrada                      AS "IdEntrada",
            t.id_solicitante                  AS "IdSolicitante",
            us."Email"                        AS "EmailSolicitante",
            t.id_receptor                     AS "IdReceptor",
            ur."Email"                        AS "EmailReceptor",
            t.estado                          AS "Estado",
            t.fecha_solicitud                 AS "FechaSolicitud",
            en.id_venta                       AS "IdVenta",
            en.nro_linea_detalle_venta        AS "NroLineaDetalleVenta",
            dv.id_evento                      AS "IdEvento",
            e.fecha_hora                      AS "FechaHoraEvento",
            est.nombre                        AS "NombreEstadio",
            eq_l.nombre                       AS "EquipoLocal",
            eq_v.nombre                       AS "EquipoVisitante",
            dv.id_sector                      AS "IdSector",
            (dv.subtotal / dv.cantidad)       AS "PrecioUnitario",
            v.estado                          AS "EstadoVenta"
        FROM transferencias t
        JOIN entradas en ON t.id_entrada = en.id_entrada
        JOIN "AspNetUsers" us ON t.id_solicitante = us."Id"
        JOIN "AspNetUsers" ur ON t.id_receptor = ur."Id"
        JOIN detalle_venta dv
            ON en.id_venta = dv.id_venta
            AND en.nro_linea_detalle_venta = dv.nro_linea
        JOIN ventas v ON en.id_venta = v.id_venta
        JOIN eventos e ON dv.id_evento = e.id_evento
        JOIN estadios est ON e.id_estadio = est.id_estadio
        LEFT JOIN equipo_juega_evento eje_l
            ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
        LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
        LEFT JOIN equipo_juega_evento eje_v
            ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
        LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
        """;

    public async Task<int> CreateSolicitudAsync(Guid idEntrada, string idSolicitante, string emailReceptor)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var normalizedEmail = emailReceptor.Trim().ToUpperInvariant();
        var idReceptor = await connection.QuerySingleOrDefaultAsync<string>(
            """
            SELECT u."Id"
            FROM "AspNetUsers" u
            JOIN usuarios_generales ug ON ug.usuario_id = u."Id"
            WHERE UPPER(u."Email") = @NormalizedEmail
            LIMIT 1
            """,
            new { NormalizedEmail = normalizedEmail },
            transaction);

        if (idReceptor is null)
            throw new InvalidOperationException("No existe un usuario general con ese email.");

        if (idReceptor == idSolicitante)
            throw new InvalidOperationException("No podés transferirte una entrada a vos mismo.");

        var idPoseedor = await connection.QuerySingleOrDefaultAsync<string>(
            """
            SELECT id_poseedor
            FROM entradas
            WHERE id_entrada = @IdEntrada
            FOR UPDATE
            """,
            new { IdEntrada = idEntrada },
            transaction);

        if (idPoseedor is null)
            throw new InvalidOperationException("No se encontró la entrada seleccionada.");

        if (idPoseedor != idSolicitante)
            throw new InvalidOperationException(
                "La entrada seleccionada no pertenece actualmente a tu usuario.");

        var tienePendiente = await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM transferencias
                WHERE id_entrada = @IdEntrada
                  AND estado = 'pendiente'
            )
            """,
            new { IdEntrada = idEntrada },
            transaction);

        if (tienePendiente)
            throw new InvalidOperationException(
                "La entrada seleccionada ya tiene una solicitud de transferencia pendiente.");

        try
        {
            var idTransferencia = await connection.ExecuteScalarAsync<int>(
                """
                INSERT INTO transferencias
                    (id_entrada, id_solicitante, id_receptor, estado, fecha_solicitud)
                VALUES
                    (@IdEntrada, @IdSolicitante, @IdReceptor, 'pendiente', NOW())
                RETURNING id_transferencia
                """,
                new
                {
                    IdEntrada = idEntrada,
                    IdSolicitante = idSolicitante,
                    IdReceptor = idReceptor
                },
                transaction);

            await transaction.CommitAsync();
            return idTransferencia;
        }
        catch (PostgresException ex)
            when (ex.SqlState == PostgresErrorCodes.UniqueViolation
                  && ex.ConstraintName == "ux_transferencias_entrada_pendiente")
        {
            throw new InvalidOperationException(
                "La entrada seleccionada ya tiene una solicitud de transferencia pendiente.",
                ex);
        }
    }

    public async Task<IEnumerable<TransferenciaDetalle>> GetTransferenciasByUsuarioAsync(string idUsuario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<TransferenciaDetalle>(
            TransferenciaDetalleSelectFrom + """

            WHERE t.id_solicitante = @IdUsuario OR t.id_receptor = @IdUsuario
            ORDER BY t.fecha_solicitud DESC, t.id_transferencia DESC
            """,
            new { IdUsuario = idUsuario });
    }

    public async Task<IEnumerable<TransferenciaDetalle>> GetPendientesRecibidasAsync(string idUsuario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<TransferenciaDetalle>(
            TransferenciaDetalleSelectFrom + """

            WHERE t.id_receptor = @IdUsuario AND t.estado = 'pendiente'
            ORDER BY t.fecha_solicitud DESC, t.id_transferencia DESC
            """,
            new { IdUsuario = idUsuario });
    }

    public async Task AcceptAsync(int idTransferencia, string idReceptor)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var transferencia = await GetTransferenciaForUpdateAsync(
            connection,
            transaction,
            idTransferencia);

        ValidarTransferenciaPendiente(transferencia, idReceptor);

        var idPoseedor = await connection.QuerySingleOrDefaultAsync<string>(
            """
            SELECT id_poseedor
            FROM entradas
            WHERE id_entrada = @IdEntrada
            FOR UPDATE
            """,
            new { transferencia.IdEntrada },
            transaction);

        if (idPoseedor != transferencia.IdSolicitante)
            throw new InvalidOperationException(
                "La entrada ya no pertenece al usuario que solicitó la transferencia.");

        await connection.ExecuteAsync(
            """
            UPDATE entradas
            SET id_poseedor = @IdReceptor
            WHERE id_entrada = @IdEntrada
            """,
            new
            {
                transferencia.IdEntrada,
                IdReceptor = idReceptor
            },
            transaction);

        await connection.ExecuteAsync(
            """
            UPDATE transferencias
            SET estado = 'aceptada'
            WHERE id_transferencia = @IdTransferencia
            """,
            new { IdTransferencia = idTransferencia },
            transaction);

        await transaction.CommitAsync();
    }

    public async Task RejectAsync(int idTransferencia, string idReceptor)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var transferencia = await GetTransferenciaForUpdateAsync(
            connection,
            transaction,
            idTransferencia);

        ValidarTransferenciaPendiente(transferencia, idReceptor);

        await connection.ExecuteAsync(
            """
            UPDATE transferencias
            SET estado = 'rechazada'
            WHERE id_transferencia = @IdTransferencia
            """,
            new { IdTransferencia = idTransferencia },
            transaction);

        await transaction.CommitAsync();
    }

    private static async Task<TransferenciaRow> GetTransferenciaForUpdateAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int idTransferencia)
    {
        var transferencia = await connection.QuerySingleOrDefaultAsync<TransferenciaRow>(
            """
            SELECT
                id_entrada      AS "IdEntrada",
                id_solicitante  AS "IdSolicitante",
                id_receptor     AS "IdReceptor",
                estado          AS "Estado"
            FROM transferencias
            WHERE id_transferencia = @IdTransferencia
            FOR UPDATE
            """,
            new { IdTransferencia = idTransferencia },
            transaction);

        if (transferencia is null)
            throw new InvalidOperationException("No se encontró la solicitud de transferencia.");

        return transferencia;
    }

    private static void ValidarTransferenciaPendiente(
        TransferenciaRow transferencia,
        string idReceptor)
    {
        if (transferencia.IdReceptor != idReceptor)
            throw new InvalidOperationException("Esta solicitud no pertenece a tu usuario.");

        if (transferencia.Estado != "pendiente")
            throw new InvalidOperationException("Esta solicitud ya fue respondida.");
    }

    private sealed class TransferenciaRow
    {
        public Guid IdEntrada { get; set; }
        public string IdSolicitante { get; set; } = "";
        public string IdReceptor { get; set; } = "";
        public string Estado { get; set; } = "";
    }
}
