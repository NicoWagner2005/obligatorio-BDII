using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Daos;

public class AuditoriaDao(IConfiguration configuration) : IAuditoriaDao
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<KpiAuditoria> GetKpiAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleAsync<KpiAuditoria>(
            """
            SELECT
                (SELECT COUNT(*)::int FROM ventas WHERE estado = 'paga')        AS "VentasPagas",
                (SELECT COUNT(*)::int FROM ventas WHERE estado = 'pendiente')   AS "VentasPendientes",
                (SELECT COUNT(*)::int FROM ventas WHERE estado = 'confirmada')  AS "VentasConfirmadas",
                (SELECT COALESCE(SUM(monto_total), 0)
                 FROM ventas WHERE estado = 'paga')                             AS "IngresosTotales",
                (SELECT COUNT(*)::int FROM entradas)                            AS "TotalEntradas",
                (SELECT COUNT(*)::int FROM entradas WHERE consumida)            AS "EntradasConsumidas",
                (SELECT COUNT(*)::int FROM transferencias)                      AS "TotalTransferencias",
                (SELECT COUNT(*)::int FROM transferencias WHERE estado = 'aceptada') AS "TransferenciasAceptadas"
            """);
    }

    public async Task<IEnumerable<RankingEvento>> GetRankingEventosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<RankingEvento>(
            """
            SELECT
                e.id_evento                                                     AS "IdEvento",
                COALESCE(eq_l.nombre, '?')                                      AS "EquipoLocal",
                COALESCE(eq_v.nombre, '?')                                      AS "EquipoVisitante",
                est.nombre                                                      AS "NombreEstadio",
                e.fecha_hora                                                    AS "FechaHora",
                COUNT(en.id_entrada)                                            AS "EntradasVendidas",
                COUNT(en.id_entrada) FILTER (WHERE en.consumida)                AS "EntradasConsumidas",
                (
                    SELECT COALESCE(SUM(dv2.subtotal), 0)
                    FROM detalle_venta dv2
                    JOIN ventas v2 ON v2.id_venta = dv2.id_venta AND v2.estado = 'paga'
                    WHERE dv2.id_evento = e.id_evento
                )                                                               AS "Ingresos",
                (
                    SELECT COALESCE(SUM(s.capacidad_max), 0)
                    FROM evento_habilita_sector ehs2
                    JOIN sectores s
                        ON s.id_estadio = ehs2.id_estadio AND s.id_sector = ehs2.id_sector
                    WHERE ehs2.id_evento = e.id_evento
                )                                                               AS "CapacidadTotal"
            FROM eventos e
            JOIN estadios est ON est.id_estadio = e.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON eje_l.id_evento = e.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eq_l.id_equipo = eje_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON eje_v.id_evento = e.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eq_v.id_equipo = eje_v.id_equipo
            LEFT JOIN detalle_venta dv ON dv.id_evento = e.id_evento
            LEFT JOIN entradas en
                ON en.id_venta = dv.id_venta AND en.nro_linea_detalle_venta = dv.nro_linea
            GROUP BY e.id_evento, eq_l.nombre, eq_v.nombre, est.nombre, e.fecha_hora
            ORDER BY COUNT(en.id_entrada) DESC
            """);
    }

    public async Task<IEnumerable<RankingComprador>> GetRankingCompradoresAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<RankingComprador>(
            """
            WITH ventas_pagas AS (
                SELECT id_venta, id_comprador, monto_total
                FROM ventas
                WHERE estado = 'paga'
            )
            SELECT
                u."Email"                                                       AS "Email",
                COUNT(vp.id_venta)                                              AS "TotalVentas",
                COALESCE(SUM(vp.monto_total), 0)                                AS "TotalGastado",
                (
                    SELECT COUNT(*)::int
                    FROM detalle_venta dv2
                    JOIN entradas en2
                        ON en2.id_venta = dv2.id_venta
                       AND en2.nro_linea_detalle_venta = dv2.nro_linea
                    WHERE dv2.id_venta IN (
                        SELECT id_venta FROM ventas_pagas vp2
                        WHERE vp2.id_comprador = vp.id_comprador
                    )
                )                                                               AS "TotalEntradas"
            FROM ventas_pagas vp
            JOIN "AspNetUsers" u ON u."Id" = vp.id_comprador
            GROUP BY vp.id_comprador, u."Email"
            ORDER BY SUM(vp.monto_total) DESC
            LIMIT 10
            """);
    }

    public async Task<IEnumerable<RankingFuncionario>> GetRankingFuncionariosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<RankingFuncionario>(
            """
            SELECT
                f.nro_legajo                        AS "NroLegajo",
                u."Email"                           AS "Email",
                COUNT(tq.id_token_qr)::int          AS "TotalValidaciones"
            FROM funcionarios f
            JOIN "AspNetUsers" u ON u."Id" = f.usuario_id
            LEFT JOIN dispositivos_escaneo d ON d.id_funcionario = f.usuario_id
            LEFT JOIN tokens_qr tq ON tq.id_dispositivo = d.id_dispositivo
            GROUP BY f.usuario_id, f.nro_legajo, u."Email"
            ORDER BY COUNT(tq.id_token_qr) DESC
            """);
    }

    public async Task<EstadisticaTransferencias> GetEstadisticaTransferenciasAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleAsync<EstadisticaTransferencias>(
            """
            SELECT
                COUNT(*) FILTER (WHERE estado = 'pendiente')::int   AS "Pendientes",
                COUNT(*) FILTER (WHERE estado = 'aceptada')::int    AS "Aceptadas",
                COUNT(*) FILTER (WHERE estado = 'rechazada')::int   AS "Rechazadas"
            FROM transferencias
            """);
    }
}
