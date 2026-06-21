using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class TokenQrService(ITokenQrDao dao)
{
    public Task<int> RenovarTokensActivosAsync() =>
        dao.RenovarTokensActivosAsync();

    public async Task<TokenQrActivo?> GetTokenActivoByEntradaAsync(string idUsuario, Guid idEntrada)
    {
        if (idEntrada == Guid.Empty)
            throw new InvalidOperationException("Debe seleccionar una entrada.");

        return await dao.GetTokenActivoByEntradaAsync(idUsuario, idEntrada);
    }
}
