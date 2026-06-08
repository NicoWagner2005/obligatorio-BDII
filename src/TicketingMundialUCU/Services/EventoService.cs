using TicketingMundialUCU.Data.Repositories;

namespace TicketingMundialUCU.Services;

public class EventoService(EventoRepository eventoRepository, EstadioRepository estadioRepository)
{
    public Task<IEnumerable<Equipo>> GetAllEquiposAsync() =>
        eventoRepository.GetAllEquiposAsync();

    public Task<IEnumerable<Estadio>> GetAllEstadiosAsync() =>
        estadioRepository.GetAllEstadiosAsync();

    public Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(int idEstadio) =>
        estadioRepository.GetSectoresByEstadioAsync(idEstadio);

    public Task<IEnumerable<EventoDetalle>> GetAllEventosDetalladosAsync() =>
        eventoRepository.GetAllEventosDetalladosAsync();

    public Task<Dictionary<int, List<SectorHabilitado>>> GetAllSectoresHabilitadosAsync() =>
        eventoRepository.GetAllSectoresHabilitadosAsync();

    public async Task AgregarEquipoAsync(string nombre)
    {
        try
        {
            await eventoRepository.CreateEquipoAsync(nombre);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate") || ex.Message.Contains("unique"))
        {
            throw new InvalidOperationException("Ya existe un equipo con ese nombre.");
        }
    }

    public async Task<int> ProgramarEventoAsync(
        DateTime fechaHora,
        string idAdministrador,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio)
    {
        var sectores = sectoresConPrecio.ToList();
        ValidarEvento(idEstadio, idEquipoLocal, idEquipoVisitante, sectores);

        if (await eventoRepository.ExisteSuperposicionAsync(idEstadio, fechaHora))
            throw new InvalidOperationException(
                "Ya existe un evento en ese estadio dentro de las 3 horas de la fecha indicada.");

        return await eventoRepository.CreateEventoAsync(
            fechaHora, idAdministrador, idEstadio, idEquipoLocal, idEquipoVisitante, sectores);
    }

    public async Task ActualizarEventoAsync(
        int idEvento,
        DateTime fechaHora,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio)
    {
        var sectores = sectoresConPrecio.ToList();
        ValidarEvento(idEstadio, idEquipoLocal, idEquipoVisitante, sectores);

        if (await eventoRepository.ExisteSuperposicionAsync(idEstadio, fechaHora, idEvento))
            throw new InvalidOperationException(
                "Ya existe un evento en ese estadio dentro de las 3 horas de la fecha indicada.");

        await eventoRepository.UpdateEventoAsync(
            idEvento, fechaHora, idEstadio, idEquipoLocal, idEquipoVisitante, sectores);
    }

    public Task EliminarEventoAsync(int idEvento) =>
        eventoRepository.DeleteEventoAsync(idEvento);

    private static void ValidarEvento(
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        List<(string Sector, decimal Precio)> sectores)
    {
        if (idEstadio == 0)
            throw new InvalidOperationException("Debe seleccionar un estadio.");
        if (idEquipoLocal == 0)
            throw new InvalidOperationException("Debe seleccionar el equipo local.");
        if (idEquipoVisitante == 0)
            throw new InvalidOperationException("Debe seleccionar el equipo visitante.");
        if (idEquipoLocal == idEquipoVisitante)
            throw new InvalidOperationException("El equipo local y visitante no pueden ser el mismo.");
        if (sectores.Count == 0)
            throw new InvalidOperationException("Debe habilitar al menos un sector para el evento.");
        if (sectores.Any(s => s.Precio <= 0))
            throw new InvalidOperationException("El precio de cada sector habilitado debe ser mayor a 0.");
    }
}
