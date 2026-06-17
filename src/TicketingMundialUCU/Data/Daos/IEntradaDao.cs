namespace TicketingMundialUCU.Data.Daos;

public interface IEntradaDao
{
    Task<IEnumerable<EntradaDetalle>> GetEntradasByUsuarioAsync(string idUsuario);
    Task<IEnumerable<EntradaDetalle>> GetDetallesByVentaAsync(int idVenta);
}
