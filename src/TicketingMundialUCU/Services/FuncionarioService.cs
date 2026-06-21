using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class FuncionarioService(
    IFuncionarioDao dao,
    ICurrentUserContext userContext)
{
    // ── Funcionarios ────────────────────────────────────────────

    public Task<IEnumerable<FuncionarioInfo>> GetAllFuncionariosAsync() =>
        dao.GetAllFuncionariosAsync();

    // ── Dispositivos ─────────────────────────────────────────────

    public Task<IEnumerable<DispositivoAutorizado>> GetAllDispositivosAsync() =>
        dao.GetAllDispositivosAsync();

    public async Task<IEnumerable<DispositivoAutorizado>> GetMisDispositivosAsync()
    {
        var id = await userContext.GetRequiredFuncionarioIdAsync();
        return await dao.GetDispositivosByFuncionarioAsync(id);
    }

    public async Task RegistrarDispositivoAsync(string idDispositivo, string idFuncionario)
    {
        if (string.IsNullOrWhiteSpace(idDispositivo))
            throw new InvalidOperationException("El ID del dispositivo es obligatorio.");

        await dao.CreateDispositivoAsync(idDispositivo.Trim(), idFuncionario);
    }

    public async Task EliminarDispositivoAsync(string idDispositivo)
    {
        if (!await dao.DeleteDispositivoAsync(idDispositivo))
            throw new InvalidOperationException("No se encontró el dispositivo.");
    }

    // ── Asignaciones ─────────────────────────────────────────────

    public Task<IEnumerable<AsignacionSector>> GetAllAsignacionesAsync() =>
        dao.GetAllAsignacionesAsync();

    public Task<IEnumerable<AsignacionSector>> GetAsignacionesByEventoAsync(int idEvento) =>
        dao.GetAsignacionesByEventoAsync(idEvento);

    public async Task<IEnumerable<AsignacionSector>> GetMisAsignacionesAsync()
    {
        var id = await userContext.GetRequiredFuncionarioIdAsync();
        return await dao.GetAsignacionesByFuncionarioAsync(id);
    }

    public async Task AsignarSectorAsync(
        string idFuncionario, int idEvento, int idEstadio, string idSector)
    {
        if (await dao.ExisteAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector))
            throw new InvalidOperationException("El funcionario ya está asignado a ese sector en ese evento.");

        await dao.CreateAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector);
    }

    public async Task EliminarAsignacionAsync(
        string idFuncionario, int idEvento, int idEstadio, string idSector)
    {
        if (!await dao.DeleteAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector))
            throw new InvalidOperationException("No se encontró la asignación.");
    }

    // ── Validación ────────────────────────────────────────────────

    public async Task ValidarEntradaAsync(Guid codigoToken, string idDispositivo)
    {
        var idFuncionario = await userContext.GetRequiredFuncionarioIdAsync();

        // RF-74: el dispositivo debe ser del funcionario
        if (!await dao.IsDispositivoDelFuncionarioAsync(idDispositivo, idFuncionario))
            throw new InvalidOperationException("El dispositivo no está autorizado para este funcionario.");

        var entrada = await dao.GetEntradaParaValidarAsync(codigoToken)
            ?? throw new InvalidOperationException("No se encontró un token QR válido.");

        if (entrada.Consumida)
            throw new InvalidOperationException("La entrada ya fue validada anteriormente.");

        // RF-75/76: el funcionario debe estar asignado al sector del evento
        if (!await dao.ExisteAsignacionAsync(idFuncionario, entrada.IdEvento, entrada.IdEstadio, entrada.IdSector))
            throw new InvalidOperationException("No tenés asignación para el sector de esta entrada en este evento.");

        if (entrada.EstadoVenta != "paga")
            throw new InvalidOperationException("La venta de esta entrada no está marcada como paga.");

        await dao.ValidarEntradaAsync(codigoToken, idFuncionario, idDispositivo);
    }

    // ── Cobertura RF-77 ───────────────────────────────────────────

    public Task<IEnumerable<CoberturasSector>> GetCoberturaSectoresAsync(
        string idFuncionario, int idEvento) =>
        dao.GetCoberturaSectoresAsync(idFuncionario, idEvento);
}
