using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Daos;

public record EntradaDetalle(
    Guid IdEntrada,
    Guid? CodigoToken,
    int IdVenta,
    int NroLineaDetalleVenta,
    int IdEvento,
    DateTime FechaHoraEvento,
    string NombreEstadio,
    string? EquipoLocal,
    string? EquipoVisitante,
    string IdSector,
    decimal PrecioUnitario,
    string EstadoVenta,
    bool Consumida);

public class EntradaDao(IConfiguration configuration) : IEntradaDao
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    private const string EntradaDetalleSelectFrom = """
        SELECT
            en.id_entrada                       AS "IdEntrada",
            CASE
                WHEN v.estado = 'paga' THEN tq.codigo_token
                ELSE NULL
            END                                 AS "CodigoToken",
            en.id_venta                         AS "IdVenta",
            en.nro_linea_detalle_venta          AS "NroLineaDetalleVenta",
            dv.id_evento                        AS "IdEvento",
            e.fecha_hora                        AS "FechaHoraEvento",
            est.nombre                          AS "NombreEstadio",
            eq_l.nombre                         AS "EquipoLocal",
            eq_v.nombre                         AS "EquipoVisitante",
            dv.id_sector                        AS "IdSector",
            (dv.subtotal / dv.cantidad)         AS "PrecioUnitario",
            v.estado                            AS "EstadoVenta",
            EXISTS (
                SELECT 1
                FROM tokens_qr tq_validado
                WHERE tq_validado.id_entrada = en.id_entrada
                  AND tq_validado.id_dispositivo IS NOT NULL
            )                                   AS "Consumida"
        FROM entradas en
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
        LEFT JOIN LATERAL (
            SELECT codigo_token
            FROM tokens_qr
            WHERE id_entrada = en.id_entrada
              AND id_dispositivo IS NULL
              AND fecha_expiracion > LOCALTIMESTAMP
            ORDER BY fecha_expiracion DESC, codigo_token
            LIMIT 1
        ) tq ON TRUE
        """;

    public async Task<IEnumerable<EntradaDetalle>> GetEntradasByUsuarioAsync(string idUsuario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<EntradaDetalle>(
            EntradaDetalleSelectFrom + """

            WHERE en.id_poseedor = @IdUsuario
            ORDER BY v.fecha_venta DESC, en.id_venta, en.nro_linea_detalle_venta, en.id_entrada
            """,
            new { IdUsuario = idUsuario });
    }

    public async Task<IEnumerable<EntradaDetalle>> GetDetallesByVentaAsync(int idVenta)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<EntradaDetalle>(
            EntradaDetalleSelectFrom + """

            WHERE en.id_venta = @IdVenta
            ORDER BY en.nro_linea_detalle_venta, en.id_entrada
            """,
            new { IdVenta = idVenta });
    }
}
