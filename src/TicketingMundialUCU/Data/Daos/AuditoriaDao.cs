using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Daos;

public class AuditoriaDao(IConfiguration configuration) : IAuditoriaDao
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<ResumenEstadisticas> GetResumenAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<ResumenEstadisticas>(
            """
            SELECT
                (SELECT COUNT(*)::int FROM ventas)                                                          AS "TotalVentas",
                (SELECT COUNT(*)::int FROM ventas WHERE estado = 'pendiente')                               AS "VentasPendientes",
                (SELECT COUNT(*)::int FROM ventas WHERE estado = 'confirmada')                              AS "VentasConfirmadas",
                (SELECT COUNT(*)::int FROM ventas WHERE estado = 'paga')                                    AS "VentasPagas",
                (SELECT COUNT(*)::int FROM entradas)                                                        AS "TotalEntradas",
                (SELECT COALESCE(SUM(monto_total), 0) FROM ventas WHERE estado != 'cancelada')              AS "RecaudacionTotal",
                (SELECT COALESCE(SUM(monto_total), 0) FROM ventas WHERE estado = 'paga')                   AS "RecaudacionPaga",
                (SELECT COUNT(*)::int FROM usuarios_generales)                                              AS "TotalUsuarios",
                (SELECT COUNT(*)::int FROM eventos)                                                         AS "TotalEventos",
                (SELECT COUNT(*)::int FROM transferencias)                                                  AS "TotalTransferencias"
            """);
    }

    public async Task<IEnumerable<EventoRanking>> GetRankingEventosAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<EventoRanking>(
            """
            SELECT
                e.id_evento                             AS "IdEvento",
                e.fecha_hora                            AS "FechaHoraEvento",
                est.nombre                              AS "NombreEstadio",
                COALESCE(eq_l.nombre, '—')              AS "EquipoLocal",
                COALESCE(eq_v.nombre, '—')              AS "EquipoVisitante",
                COALESCE(vendidas.total, 0)             AS "EntradasVendidas",
                COALESCE(vendidas.recaudacion, 0)       AS "Recaudacion",
                COALESCE(cap.capacidad_total, 0)        AS "CapacidadTotal"
            FROM eventos e
            JOIN estadios est ON est.id_estadio = e.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            LEFT JOIN (
                SELECT dv.id_evento,
                       COUNT(en.id_entrada)::int    AS total,
                       COALESCE(SUM(dv.subtotal), 0)  AS recaudacion
                FROM detalle_venta dv
                JOIN ventas v ON v.id_venta = dv.id_venta AND v.estado != 'cancelada'
                JOIN entradas en
                    ON en.id_venta = dv.id_venta
                    AND en.nro_linea_detalle_venta = dv.nro_linea
                GROUP BY dv.id_evento
            ) vendidas ON vendidas.id_evento = e.id_evento
            LEFT JOIN (
                SELECT ehs.id_evento, SUM(s.capacidad_max)::int AS capacidad_total
                FROM evento_habilita_sector ehs
                JOIN sectores s
                    ON s.id_estadio = ehs.id_estadio AND s.id_sector = ehs.id_sector
                GROUP BY ehs.id_evento
            ) cap ON cap.id_evento = e.id_evento
            ORDER BY "EntradasVendidas" DESC, e.fecha_hora
            """);
    }

    public async Task<IEnumerable<CompradorRanking>> GetRankingCompradoresAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<CompradorRanking>(
            """
            WITH venta_stats AS (
                SELECT
                    v.id_comprador,
                    COUNT(DISTINCT v.id_venta)::int      AS total_ventas,
                    COALESCE(SUM(v.monto_total), 0)      AS total_gastado
                FROM ventas v
                WHERE v.estado != 'cancelada'
                GROUP BY v.id_comprador
            ),
            entrada_stats AS (
                SELECT
                    v.id_comprador,
                    COUNT(en.id_entrada)::int AS total_entradas
                FROM ventas v
                JOIN detalle_venta dv ON dv.id_venta = v.id_venta
                JOIN entradas en
                    ON en.id_venta = dv.id_venta
                    AND en.nro_linea_detalle_venta = dv.nro_linea
                WHERE v.estado != 'cancelada'
                GROUP BY v.id_comprador
            )
            SELECT
                u."Email"                   AS "EmailUsuario",
                vs.total_ventas             AS "TotalVentas",
                vs.total_gastado            AS "TotalGastado",
                es.total_entradas           AS "TotalEntradas"
            FROM "AspNetUsers" u
            JOIN venta_stats vs  ON vs.id_comprador  = u."Id"
            JOIN entrada_stats es ON es.id_comprador = u."Id"
            ORDER BY es.total_entradas DESC, vs.total_gastado DESC
            LIMIT 20
            """);
    }

    public async Task<IEnumerable<OcupacionSector>> GetOcupacionSectoresAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<OcupacionSector>(
            """
            SELECT
                e.id_evento                                                             AS "IdEvento",
                e.fecha_hora                                                            AS "FechaHoraEvento",
                COALESCE(eq_l.nombre, '—')                                              AS "EquipoLocal",
                COALESCE(eq_v.nombre, '—')                                              AS "EquipoVisitante",
                est.nombre                                                              AS "NombreEstadio",
                ehs.id_sector                                                           AS "IdSector",
                s.capacidad_max                                                         AS "CapacidadMax",
                COALESCE(vendidas.total, 0)                                             AS "EntradasVendidas",
                ROUND(
                    COALESCE(vendidas.total, 0) * 100.0 / NULLIF(s.capacidad_max, 0),
                    1
                )                                                                       AS "PorcentajeOcupacion"
            FROM evento_habilita_sector ehs
            JOIN eventos e ON e.id_evento = ehs.id_evento
            JOIN estadios est ON est.id_estadio = ehs.id_estadio
            JOIN sectores s ON s.id_estadio = ehs.id_estadio AND s.id_sector = ehs.id_sector
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            LEFT JOIN (
                SELECT dv.id_evento, dv.id_estadio, dv.id_sector,
                       COUNT(en.id_entrada)::int AS total
                FROM detalle_venta dv
                JOIN ventas v ON v.id_venta = dv.id_venta AND v.estado != 'cancelada'
                JOIN entradas en
                    ON en.id_venta = dv.id_venta
                    AND en.nro_linea_detalle_venta = dv.nro_linea
                GROUP BY dv.id_evento, dv.id_estadio, dv.id_sector
            ) vendidas
                ON vendidas.id_evento  = ehs.id_evento
               AND vendidas.id_estadio = ehs.id_estadio
               AND vendidas.id_sector  = ehs.id_sector
            ORDER BY e.fecha_hora, ehs.id_sector
            """);
    }

    public async Task<IEnumerable<MovimientoAuditoria>> GetHistorialCustodiaAsync(int limite = 100)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<MovimientoAuditoria>(
            """
            SELECT
                hce.id_movimiento                   AS "IdMovimiento",
                hce.id_entrada                      AS "IdEntrada",
                hce.tipo_movimiento                 AS "TipoMovimiento",
                hce.fecha_movimiento                AS "FechaMovimiento",
                u."Email"                           AS "EmailPoseedorActual",
                COALESCE(eq_l.nombre, '—')          AS "EquipoLocal",
                COALESCE(eq_v.nombre, '—')          AS "EquipoVisitante",
                e.fecha_hora                        AS "FechaHoraEvento"
            FROM historial_custodia_entrada hce
            JOIN entradas en ON en.id_entrada = hce.id_entrada
            JOIN "AspNetUsers" u ON u."Id" = en.id_poseedor
            JOIN detalle_venta dv
                ON dv.id_venta = en.id_venta
                AND dv.nro_linea = en.nro_linea_detalle_venta
            JOIN eventos e ON e.id_evento = dv.id_evento
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            ORDER BY hce.fecha_movimiento DESC
            LIMIT @Limite
            """,
            new { Limite = limite });
    }
}
