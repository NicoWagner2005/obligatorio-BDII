using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Daos;

public class TokenQrDao(IConfiguration configuration) : ITokenQrDao
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> RenovarTokensActivosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var entradas = (await connection.QueryAsync<Guid>(
            """
            SELECT en.id_entrada
            FROM entradas en
            JOIN ventas v ON v.id_venta = en.id_venta
            JOIN detalle_venta dv
                ON dv.id_venta = en.id_venta
               AND dv.nro_linea = en.nro_linea_detalle_venta
            JOIN eventos e ON e.id_evento = dv.id_evento
            WHERE v.estado = 'paga'
              AND e.fecha_hora + interval '4 hours' >= LOCALTIMESTAMP
              AND NOT EXISTS (
                  SELECT 1
                  FROM tokens_qr tq_validado
                  WHERE tq_validado.id_entrada = en.id_entrada
                    AND tq_validado.id_dispositivo IS NOT NULL
              )
            ORDER BY en.id_entrada
            FOR UPDATE OF en
            """,
            transaction)).ToList();

        if (entradas.Count == 0)
        {
            await transaction.CommitAsync();
            return 0;
        }

        var tokensNuevos = entradas
            .Select(idEntrada => new
            {
                CodigoToken = Guid.NewGuid(),
                IdEntrada = idEntrada,
            })
            .ToList();

        await connection.ExecuteAsync(
            """
            INSERT INTO tokens_qr (codigo_token, id_entrada, fecha_expiracion, id_dispositivo)
            VALUES (@CodigoToken, @IdEntrada, LOCALTIMESTAMP + interval '30 seconds', NULL)
            ON CONFLICT (id_entrada) WHERE id_dispositivo IS NULL
            DO UPDATE SET
                codigo_token = EXCLUDED.codigo_token,
                fecha_expiracion = EXCLUDED.fecha_expiracion
            """,
            tokensNuevos,
            transaction);

        await transaction.CommitAsync();
        return entradas.Count;
    }

    public async Task<TokenQrActivo?> GetTokenActivoByEntradaAsync(string idUsuario, Guid idEntrada)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<TokenQrActivo>(
            """
            SELECT
                tq.id_entrada       AS "IdEntrada",
                tq.codigo_token     AS "CodigoToken",
                tq.fecha_expiracion AS "FechaExpiracion"
            FROM tokens_qr tq
            JOIN entradas en ON en.id_entrada = tq.id_entrada
            JOIN ventas v ON v.id_venta = en.id_venta
            WHERE tq.id_entrada = @IdEntrada
              AND en.id_poseedor = @IdUsuario
              AND v.estado = 'paga'
              AND tq.id_dispositivo IS NULL
              AND tq.fecha_expiracion > LOCALTIMESTAMP
            ORDER BY tq.fecha_expiracion DESC, tq.codigo_token
            LIMIT 1
            """,
            new { IdUsuario = idUsuario, IdEntrada = idEntrada });
    }
}
