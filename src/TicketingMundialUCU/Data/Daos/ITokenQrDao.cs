namespace TicketingMundialUCU.Data.Daos;

public record TokenQrActivo(Guid IdEntrada, Guid CodigoToken, DateTime FechaExpiracion);

public interface ITokenQrDao
{
    Task<int> RenovarTokensActivosAsync();
    Task<TokenQrActivo?> GetTokenActivoByEntradaAsync(string idUsuario, Guid idEntrada);
}
