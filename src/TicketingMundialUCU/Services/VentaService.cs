using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class VentaService(IVentaDao dao, IEntradaDao entradaDao)
{
    public Task<TasaComision> GetTasaVigenteAsync() =>
        dao.GetTasaVigenteAsync();

    public Task<Dictionary<string, int>> GetDisponibilidadAsync(int idEvento, int idEstadio) =>
        dao.GetDisponibilidadAsync(idEvento, idEstadio);

    public Task<IEnumerable<EntradaDetalle>> GetEntradasByUsuarioAsync(string idUsuario) =>
        entradaDao.GetEntradasByUsuarioAsync(idUsuario);

    public Task<IEnumerable<VentaResumen>> GetAllVentasAsync() =>
        dao.GetAllVentasAsync();

    public Task<IEnumerable<VentaResumen>> GetVentasByUsuarioAsync(string idUsuario) =>
        dao.GetVentasByUsuarioAsync(idUsuario);

    public Task<IEnumerable<EntradaDetalle>> GetDetallesByVentaAsync(int idVenta) =>
        entradaDao.GetDetallesByVentaAsync(idVenta);

    public async Task<int> ComprarEntradasAsync(string idUsuario, List<ItemCarrito> items)
    {
        var itemsConCantidad = items.Where(i => i.Cantidad > 0).ToList();

        if (itemsConCantidad.Count == 0)
            throw new InvalidOperationException("Debe seleccionar al menos una entrada.");

        if (itemsConCantidad.Sum(i => i.Cantidad) > 5)
            throw new InvalidOperationException("No se pueden comprar más de 5 entradas por transacción.");

        var tasa = await dao.GetTasaVigenteAsync();
        return await dao.CreateVentaAsync(idUsuario, itemsConCantidad, tasa);
    }

    public async Task ActualizarEstadoVentaAsync(int idVenta, string nuevoEstado)
    {
        var estadosValidos = new HashSet<string> { "confirmada", "paga" };
        if (!estadosValidos.Contains(nuevoEstado))
            throw new InvalidOperationException($"El estado '{nuevoEstado}' no es válido.");

        await dao.UpdateEstadoVentaAsync(idVenta, nuevoEstado);
    }
}
