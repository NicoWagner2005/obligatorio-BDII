namespace TicketingMundialUCU.Data.Daos;

public interface ITransferenciaDao
{
    Task<int> CreateSolicitudAsync(Guid idEntrada, string idSolicitante, string emailReceptor);
    Task<IEnumerable<TransferenciaDetalle>> GetTransferenciasByUsuarioAsync(string idUsuario);
    Task<IEnumerable<TransferenciaDetalle>> GetPendientesRecibidasAsync(string idUsuario);
    Task<Dictionary<Guid, int>> GetCantidadTransferenciasEfectivasAsync(IEnumerable<Guid> idsEntrada);
    Task AcceptAsync(int idTransferencia, string idReceptor);
    Task RejectAsync(int idTransferencia, string idReceptor);
}
