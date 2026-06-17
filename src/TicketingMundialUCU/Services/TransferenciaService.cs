using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class TransferenciaService(ITransferenciaDao dao)
{
    public Task<IEnumerable<TransferenciaDetalle>> GetTransferenciasByUsuarioAsync(string idUsuario) =>
        dao.GetTransferenciasByUsuarioAsync(idUsuario);

    public Task<IEnumerable<TransferenciaDetalle>> GetPendientesRecibidasAsync(string idUsuario) =>
        dao.GetPendientesRecibidasAsync(idUsuario);

    public async Task<int> SolicitarTransferenciaAsync(
        string idSolicitante,
        Guid idEntrada,
        string emailReceptor)
    {
        if (idEntrada == Guid.Empty)
            throw new InvalidOperationException("Debe seleccionar una entrada.");

        var email = emailReceptor.Trim();
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Debe ingresar el email del usuario receptor.");

        return await dao.CreateSolicitudAsync(idEntrada, idSolicitante, email);
    }

    public async Task AceptarTransferenciaAsync(int idTransferencia, string idReceptor)
    {
        ValidarIdTransferencia(idTransferencia);
        await dao.AcceptAsync(idTransferencia, idReceptor);
    }

    public async Task RechazarTransferenciaAsync(int idTransferencia, string idReceptor)
    {
        ValidarIdTransferencia(idTransferencia);
        await dao.RejectAsync(idTransferencia, idReceptor);
    }

    private static void ValidarIdTransferencia(int idTransferencia)
    {
        if (idTransferencia <= 0)
            throw new InvalidOperationException("La solicitud de transferencia no es válida.");
    }
}
