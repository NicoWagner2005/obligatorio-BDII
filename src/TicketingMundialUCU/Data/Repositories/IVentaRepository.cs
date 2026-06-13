namespace TicketingMundialUCU.Data.Repositories;

public interface IVentaRepository
{
    Task<TasaComision> GetTasaVigenteAsync();
    Task<int> CreateVentaAsync(
        string idUsuario,
        IEnumerable<ItemCarrito> items,
        TasaComision tasa);
    Task<IEnumerable<VentaResumen>> GetAllVentasAsync();
    Task<Dictionary<string, int>> GetDisponibilidadAsync(int idEvento, int idEstadio);
    Task UpdateEstadoVentaAsync(int idVenta, string estado);
}
