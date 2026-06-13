using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public record TasaComision(int IdTasa, decimal Tasa, DateTime FechaDesde);

public record VentaResumen(
    int IdVenta,
    string IdUsuario,
    string EmailUsuario,
    DateTime FechaVenta,
    string Estado,
    decimal MontoTotal,
    decimal TasaComisionAplicada,
    int CantidadEntradas);

public record EntradaDetalle(
    int IdDetalle,
    int IdVenta,
    int IdEvento,
    DateTime FechaHoraEvento,
    string NombreEstadio,
    string? EquipoLocal,
    string? EquipoVisitante,
    string IdSector,
    decimal PrecioUnitario,
    Guid CodigoEntrada,
    string EstadoVenta);

public record ItemCarrito(int IdEvento, int IdEstadio, string IdSector, int Cantidad);

public class VentaRepository(IConfiguration configuration) : IVentaRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task<TasaComision> GetTasaVigenteAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<TasaComision>(
            """
            SELECT id_tasa AS "IdTasa", tasa AS "Tasa", fecha_desde AS "FechaDesde"
            FROM tasa_comision
            ORDER BY fecha_desde DESC
            LIMIT 1
            """);
    }

    public async Task<int> CreateVentaAsync(
        string idUsuario,
        IEnumerable<ItemCarrito> items,
        TasaComision tasa)
    {
        var itemsList = items.ToList();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        // Obtener precios y verificar capacidad disponible (RF-25)
        var precios = new Dictionary<(int, int, string), decimal>();
        foreach (var item in itemsList)
        {
            var precio = await connection.ExecuteScalarAsync<decimal>(
                """
                SELECT precio FROM evento_habilita_sector
                WHERE id_evento = @IdEvento AND id_estadio = @IdEstadio AND id_sector = @IdSector
                """,
                new { item.IdEvento, item.IdEstadio, item.IdSector },
                transaction);
            precios[(item.IdEvento, item.IdEstadio, item.IdSector)] = precio;

            var capacidadMax = await connection.ExecuteScalarAsync<int>(
                """
                SELECT capacidad_max FROM sectores
                WHERE id_estadio = @IdEstadio AND id_sector = @IdSector
                """,
                new { item.IdEstadio, item.IdSector },
                transaction);

            var vendidas = await connection.ExecuteScalarAsync<int>(
                """
                SELECT COUNT(*) FROM detalle_venta dv
                JOIN ventas v ON dv.id_venta = v.id_venta
                WHERE dv.id_evento = @IdEvento
                  AND dv.id_estadio = @IdEstadio
                  AND dv.id_sector = @IdSector
                  AND v.estado != 'cancelada'
                """,
                new { item.IdEvento, item.IdEstadio, item.IdSector },
                transaction);

            if (vendidas + item.Cantidad > capacidadMax)
            {
                var disponibles = capacidadMax - vendidas;
                throw new InvalidOperationException(
                    $"Sector {item.IdSector}: capacidad insuficiente. " +
                    $"Disponibles: {disponibles}, solicitadas: {item.Cantidad}.");
            }
        }

        decimal subtotal = itemsList.Sum(i => i.Cantidad * precios[(i.IdEvento, i.IdEstadio, i.IdSector)]);
        decimal montoTotal = subtotal * (1 + tasa.Tasa);

        var idVenta = await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO ventas (id_usuario, fecha_venta, estado, monto_total, tasa_comision_aplicada, id_tasa)
            VALUES (@IdUsuario, NOW(), 'pendiente', @MontoTotal, @Tasa, @IdTasa)
            RETURNING id_venta
            """,
            new
            {
                IdUsuario = idUsuario,
                MontoTotal = montoTotal,
                Tasa = tasa.Tasa,
                IdTasa = tasa.IdTasa,
            },
            transaction);

        // RF-45/RF-46: una fila por entrada individual con codigo uuid único
        foreach (var item in itemsList)
        {
            var precioUnit = precios[(item.IdEvento, item.IdEstadio, item.IdSector)];
            for (int i = 0; i < item.Cantidad; i++)
            {
                await connection.ExecuteAsync(
                    """
                    INSERT INTO detalle_venta
                        (id_venta, id_evento, id_estadio, id_sector, precio_unitario, codigo_entrada)
                    VALUES
                        (@IdVenta, @IdEvento, @IdEstadio, @IdSector, @PrecioUnitario, gen_random_uuid())
                    """,
                    new
                    {
                        IdVenta = idVenta,
                        item.IdEvento,
                        item.IdEstadio,
                        item.IdSector,
                        PrecioUnitario = precioUnit,
                    },
                    transaction);
            }
        }

        await transaction.CommitAsync();
        return idVenta;
    }

    public async Task<IEnumerable<EntradaDetalle>> GetEntradasByUsuarioAsync(string idUsuario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<EntradaDetalle>(
            """
            SELECT
                dv.id_detalle           AS "IdDetalle",
                dv.id_venta             AS "IdVenta",
                dv.id_evento            AS "IdEvento",
                e.fecha_hora            AS "FechaHoraEvento",
                est.nombre              AS "NombreEstadio",
                eq_l.nombre             AS "EquipoLocal",
                eq_v.nombre             AS "EquipoVisitante",
                dv.id_sector            AS "IdSector",
                dv.precio_unitario      AS "PrecioUnitario",
                dv.codigo_entrada       AS "CodigoEntrada",
                v.estado                AS "EstadoVenta"
            FROM detalle_venta dv
            JOIN ventas v ON dv.id_venta = v.id_venta
            JOIN eventos e ON dv.id_evento = e.id_evento
            JOIN estadios est ON e.id_estadio = est.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            WHERE v.id_usuario = @IdUsuario
            ORDER BY v.fecha_venta DESC, dv.id_detalle
            """,
            new { IdUsuario = idUsuario });
    }

    public async Task<IEnumerable<VentaResumen>> GetAllVentasAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<VentaResumen>(
            """
            SELECT
                v.id_venta                  AS "IdVenta",
                v.id_usuario                AS "IdUsuario",
                u."Email"                   AS "EmailUsuario",
                v.fecha_venta               AS "FechaVenta",
                v.estado                    AS "Estado",
                v.monto_total               AS "MontoTotal",
                v.tasa_comision_aplicada    AS "TasaComisionAplicada",
                COUNT(dv.id_detalle)::int   AS "CantidadEntradas"
            FROM ventas v
            JOIN "AspNetUsers" u ON v.id_usuario = u."Id"
            LEFT JOIN detalle_venta dv ON v.id_venta = dv.id_venta
            GROUP BY v.id_venta, u."Email"
            ORDER BY v.fecha_venta DESC
            """);
    }

    public async Task<IEnumerable<VentaResumen>> GetVentasByUsuarioAsync(string idUsuario)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<VentaResumen>(
            """
            SELECT
                v.id_venta                  AS "IdVenta",
                v.id_usuario                AS "IdUsuario",
                u."Email"                   AS "EmailUsuario",
                v.fecha_venta               AS "FechaVenta",
                v.estado                    AS "Estado",
                v.monto_total               AS "MontoTotal",
                v.tasa_comision_aplicada    AS "TasaComisionAplicada",
                COUNT(dv.id_detalle)::int   AS "CantidadEntradas"
            FROM ventas v
            JOIN "AspNetUsers" u ON v.id_usuario = u."Id"
            LEFT JOIN detalle_venta dv ON v.id_venta = dv.id_venta
            WHERE v.id_usuario = @IdUsuario
            GROUP BY v.id_venta, u."Email"
            ORDER BY v.fecha_venta DESC
            """,
            new { IdUsuario = idUsuario });
    }

    public async Task<IEnumerable<EntradaDetalle>> GetDetallesByVentaAsync(int idVenta)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<EntradaDetalle>(
            """
            SELECT
                dv.id_detalle           AS "IdDetalle",
                dv.id_venta             AS "IdVenta",
                dv.id_evento            AS "IdEvento",
                e.fecha_hora            AS "FechaHoraEvento",
                est.nombre              AS "NombreEstadio",
                eq_l.nombre             AS "EquipoLocal",
                eq_v.nombre             AS "EquipoVisitante",
                dv.id_sector            AS "IdSector",
                dv.precio_unitario      AS "PrecioUnitario",
                dv.codigo_entrada       AS "CodigoEntrada",
                v.estado                AS "EstadoVenta"
            FROM detalle_venta dv
            JOIN ventas v ON dv.id_venta = v.id_venta
            JOIN eventos e ON dv.id_evento = e.id_evento
            JOIN estadios est ON e.id_estadio = est.id_estadio
            LEFT JOIN equipo_juega_evento eje_l
                ON e.id_evento = eje_l.id_evento AND eje_l.rol = 'local'
            LEFT JOIN equipos eq_l ON eje_l.id_equipo = eq_l.id_equipo
            LEFT JOIN equipo_juega_evento eje_v
                ON e.id_evento = eje_v.id_evento AND eje_v.rol = 'visitante'
            LEFT JOIN equipos eq_v ON eje_v.id_equipo = eq_v.id_equipo
            WHERE dv.id_venta = @IdVenta
            ORDER BY dv.id_detalle
            """,
            new { IdVenta = idVenta });
    }

    // Retorna cuántas entradas quedan disponibles por sector para un evento (RF-25)
    public async Task<Dictionary<string, int>> GetDisponibilidadAsync(int idEvento, int idEstadio)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var rows = await connection.QueryAsync<(string IdSector, int CapacidadMax, int Vendidas)>(
            """
            SELECT
                s.id_sector                                     AS "IdSector",
                s.capacidad_max                                 AS "CapacidadMax",
                COALESCE(COUNT(dv.id_detalle), 0)::int          AS "Vendidas"
            FROM sectores s
            LEFT JOIN evento_habilita_sector ehs
                ON ehs.id_estadio = s.id_estadio AND ehs.id_sector = s.id_sector
                AND ehs.id_evento = @IdEvento
            LEFT JOIN detalle_venta dv
                ON dv.id_evento = @IdEvento
                AND dv.id_estadio = s.id_estadio
                AND dv.id_sector = s.id_sector
                AND EXISTS (
                    SELECT 1 FROM ventas v
                    WHERE v.id_venta = dv.id_venta AND v.estado != 'cancelada'
                )
            WHERE s.id_estadio = @IdEstadio
            GROUP BY s.id_sector, s.capacidad_max
            """,
            new { IdEvento = idEvento, IdEstadio = idEstadio });

        return rows.ToDictionary(r => r.IdSector, r => r.CapacidadMax - r.Vendidas);
    }

    public async Task UpdateEstadoVentaAsync(int idVenta, string estado)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            """UPDATE ventas SET estado = @Estado WHERE id_venta = @IdVenta""",
            new { IdVenta = idVenta, Estado = estado });
    }
}
