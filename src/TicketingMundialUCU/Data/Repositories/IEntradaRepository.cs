namespace TicketingMundialUCU.Data.Repositories;

public interface IEntradaRepository
{
    Task<IEnumerable<EntradaDetalle>> GetEntradasByUsuarioAsync(string idUsuario);
    Task<IEnumerable<EntradaDetalle>> GetDetallesByVentaAsync(int idVenta);
}
