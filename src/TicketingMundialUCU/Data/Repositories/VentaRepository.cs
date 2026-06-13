using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public record TasaComision(int IdTasa, decimal Tasa, DateTime FechaDesde);

public record VentaResumen(
    int IdVenta,
    string IdComprador,
    string EmailUsuario,
    DateTime FechaVenta,
    string Estado,
    decimal MontoTotal,
    decimal TasaComisionAplicada,
    int CantidadEntradas);

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
        }

        // Verificar capacidad disponible (RF-25)
        foreach (var item in itemsList.GroupBy(i => new { i.IdEvento, i.IdEstadio, i.IdSector }))
        {
            var capacidadMax = await connection.ExecuteScalarAsync<int>(
                """
                SELECT capacidad_max FROM sectores
                WHERE id_estadio = @IdEstadio AND id_sector = @IdSector
                """,
                new { item.Key.IdEstadio, item.Key.IdSector },
                transaction);

            var vendidas = await connection.ExecuteScalarAsync<int>(
                """
                SELECT COUNT(*)
                FROM entradas en
                JOIN detalle_venta dv
                    ON en.id_venta = dv.id_venta
                    AND en.nro_linea_detalle_venta = dv.nro_linea
                JOIN ventas v ON dv.id_venta = v.id_venta
                WHERE dv.id_evento = @IdEvento
                  AND dv.id_estadio = @IdEstadio
                  AND dv.id_sector = @IdSector
                  AND v.estado != 'cancelada'
                """,
                new { item.Key.IdEvento, item.Key.IdEstadio, item.Key.IdSector },
                transaction);

            var solicitadas = item.Sum(i => i.Cantidad);
            if (vendidas + solicitadas > capacidadMax)
            {
                var disponibles = capacidadMax - vendidas;
                throw new InvalidOperationException(
                    $"Sector {item.Key.IdSector}: capacidad insuficiente. " +
                    $"Disponibles: {disponibles}, solicitadas: {solicitadas}.");
            }
        }

        decimal subtotal = itemsList.Sum(i => i.Cantidad * precios[(i.IdEvento, i.IdEstadio, i.IdSector)]);
        decimal montoTotal = subtotal * (1 + tasa.Tasa);

        var idVenta = await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO ventas (id_comprador, fecha_venta, estado, monto_total, tasa_comision_aplicada, id_tasa)
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

        // RF-45/RF-46: se emite una entrada individual por boleto comprado.
        var nroLinea = 1;
        foreach (var item in itemsList)
        {
            var precioUnit = precios[(item.IdEvento, item.IdEstadio, item.IdSector)];
            var subtotalItem = precioUnit * item.Cantidad;

            await connection.ExecuteAsync(
                """
                INSERT INTO detalle_venta
                    (id_venta, nro_linea, id_evento, id_estadio, id_sector, cantidad, subtotal)
                VALUES
                    (@IdVenta, @NroLinea, @IdEvento, @IdEstadio, @IdSector, @Cantidad, @Subtotal)
                """,
                new
                {
                    IdVenta = idVenta,
                    NroLinea = nroLinea,
                    item.IdEvento,
                    item.IdEstadio,
                    item.IdSector,
                    item.Cantidad,
                    Subtotal = subtotalItem,
                },
                transaction);

            for (var i = 0; i < item.Cantidad; i++)
            {
                await connection.ExecuteAsync(
                    """
                    INSERT INTO entradas
                        (id_entrada, id_poseedor, id_venta, nro_linea_detalle_venta)
                    VALUES
                        (@IdEntrada, @IdUsuario, @IdVenta, @NroLinea)
                    """,
                    new
                    {
                        IdEntrada = Guid.NewGuid(),
                        IdUsuario = idUsuario,
                        IdVenta = idVenta,
                        NroLinea = nroLinea,
                    },
                    transaction);
            }

            nroLinea++;
        }

        await transaction.CommitAsync();
        return idVenta;
    }

    public async Task<IEnumerable<VentaResumen>> GetAllVentasAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<VentaResumen>(
            """
            SELECT
                v.id_venta                  AS "IdVenta",
                v.id_comprador              AS "IdComprador",
                u."Email"                   AS "EmailUsuario",
                v.fecha_venta               AS "FechaVenta",
                v.estado                    AS "Estado",
                v.monto_total               AS "MontoTotal",
                v.tasa_comision_aplicada    AS "TasaComisionAplicada",
                COUNT(en.id_entrada)::int   AS "CantidadEntradas"
            FROM ventas v
            JOIN "AspNetUsers" u ON v.id_comprador = u."Id"
            LEFT JOIN detalle_venta dv ON v.id_venta = dv.id_venta
            LEFT JOIN entradas en
                ON en.id_venta = dv.id_venta
                AND en.nro_linea_detalle_venta = dv.nro_linea
            GROUP BY v.id_venta, u."Email"
            ORDER BY v.fecha_venta DESC
            """);
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
                COALESCE(COUNT(en.id_entrada), 0)::int          AS "Vendidas"
            FROM sectores s
            LEFT JOIN evento_habilita_sector ehs
                ON ehs.id_estadio = s.id_estadio AND ehs.id_sector = s.id_sector
                AND ehs.id_evento = @IdEvento
            LEFT JOIN detalle_venta dv
                ON dv.id_evento = @IdEvento
                AND dv.id_estadio = s.id_estadio
                AND dv.id_sector = s.id_sector
            LEFT JOIN entradas en
                ON en.id_venta = dv.id_venta
                AND en.nro_linea_detalle_venta = dv.nro_linea
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
