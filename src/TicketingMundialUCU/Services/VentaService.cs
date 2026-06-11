using TicketingMundialUCU.Data.Repositories;

namespace TicketingMundialUCU.Services;

public class VentaService(IVentaRepository repository)
{
    public Task<TasaComision> GetTasaVigenteAsync() =>
        repository.GetTasaVigenteAsync();

    public Task<Dictionary<string, int>> GetDisponibilidadAsync(int idEvento, int idEstadio) =>
        repository.GetDisponibilidadAsync(idEvento, idEstadio);

    public Task<IEnumerable<EntradaDetalle>> GetEntradasByUsuarioAsync(string idUsuario) =>
        repository.GetEntradasByUsuarioAsync(idUsuario);

    public Task<IEnumerable<VentaResumen>> GetAllVentasAsync() =>
        repository.GetAllVentasAsync();

    public Task<IEnumerable<EntradaDetalle>> GetDetallesByVentaAsync(int idVenta) =>
        repository.GetDetallesByVentaAsync(idVenta);

    public async Task<int> ComprarEntradasAsync(string idUsuario, List<ItemCarrito> items)
    {
        var itemsConCantidad = items.Where(i => i.Cantidad > 0).ToList();

        if (itemsConCantidad.Count == 0)
            throw new InvalidOperationException("Debe seleccionar al menos una entrada.");

        if (itemsConCantidad.Sum(i => i.Cantidad) > 5)
            throw new InvalidOperationException("No se pueden comprar más de 5 entradas por transacción.");

        var tasa = await repository.GetTasaVigenteAsync();
        return await repository.CreateVentaAsync(idUsuario, itemsConCantidad, tasa);
    }

    public async Task ActualizarEstadoVentaAsync(int idVenta, string nuevoEstado)
    {
        var estadosValidos = new HashSet<string> { "confirmada", "paga" };
        if (!estadosValidos.Contains(nuevoEstado))
            throw new InvalidOperationException($"El estado '{nuevoEstado}' no es válido.");

        await repository.UpdateEstadoVentaAsync(idVenta, nuevoEstado);
    }
}
