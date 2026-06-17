namespace TicketingMundialUCU.Data.Daos;

public interface IVentaDao
{
    Task<TasaComision> GetTasaVigenteAsync();
    Task<int> CreateVentaAsync(
        string idUsuario,
        IEnumerable<ItemCarrito> items,
        TasaComision tasa);
    Task<IEnumerable<VentaResumen>> GetAllVentasAsync();
    Task<IEnumerable<VentaResumen>> GetVentasByUsuarioAsync(string idUsuario);
    Task<Dictionary<string, int>> GetDisponibilidadAsync(int idEvento, int idEstadio);
    Task UpdateEstadoVentaAsync(int idVenta, string estado);
}
