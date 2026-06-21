namespace TicketingMundialUCU.Data.Daos;

public record FuncionarioInfo(string UsuarioId, string NroLegajo, string Email);

public record DispositivoAutorizado(
    string IdDispositivo,
    string IdFuncionario,
    string NroLegajo,
    bool TieneValidaciones);

public record AsignacionSector(
    string IdFuncionario,
    string NroLegajo,
    int IdEvento,
    int IdEstadio,
    string IdSector,
    string NombreEstadio,
    DateTime FechaHora,
    string? EquipoLocal,
    string? EquipoVisitante);

public record EntradaValidacionInfo(
    Guid CodigoToken,
    Guid IdEntrada,
    bool Consumida,
    int IdEvento,
    int IdEstadio,
    string IdSector,
    string EstadoVenta);

public record CoberturasSector(string IdSector, bool Validado);

public interface IFuncionarioDao
{
    // Funcionarios
    Task<IEnumerable<FuncionarioInfo>> GetAllFuncionariosAsync();

    // Dispositivos
    Task<IEnumerable<DispositivoAutorizado>> GetAllDispositivosAsync();
    Task<IEnumerable<DispositivoAutorizado>> GetDispositivosByFuncionarioAsync(string idFuncionario);
    Task<bool> IsDispositivoDelFuncionarioAsync(string idDispositivo, string idFuncionario);
    Task CreateDispositivoAsync(string idDispositivo, string idFuncionario);
    Task<bool> DeleteDispositivoAsync(string idDispositivo);

    // Asignaciones
    Task<IEnumerable<AsignacionSector>> GetAllAsignacionesAsync();
    Task<IEnumerable<AsignacionSector>> GetAsignacionesByEventoAsync(int idEvento);
    Task<IEnumerable<AsignacionSector>> GetAsignacionesByFuncionarioAsync(string idFuncionario);
    Task<bool> ExisteAsignacionAsync(string idFuncionario, int idEvento, int idEstadio, string idSector);
    Task CreateAsignacionAsync(string idFuncionario, int idEvento, int idEstadio, string idSector);
    Task<bool> DeleteAsignacionAsync(string idFuncionario, int idEvento, int idEstadio, string idSector);

    // Validación
    Task<EntradaValidacionInfo?> GetEntradaParaValidarAsync(Guid codigoToken);
    Task ValidarEntradaAsync(Guid codigoToken, string idFuncionario, string idDispositivo);

    // Cobertura RF-77
    Task<IEnumerable<CoberturasSector>> GetCoberturaSectoresAsync(string idFuncionario, int idEvento);
}
