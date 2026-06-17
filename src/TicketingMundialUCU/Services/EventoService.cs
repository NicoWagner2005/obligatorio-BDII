using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class EventoService(
    IEventoDao eventoDao,
    IEstadioDao estadioDao,
    AdministratorJurisdictionService jurisdictionService,
    Func<DateTime>? nowProvider = null)
{
    public Task<string> GetCurrentCountryAsync() =>
        jurisdictionService.GetCurrentCountryAsync();

    public async Task<IEnumerable<Equipo>> GetAllEquiposAsync()
    {
        await jurisdictionService.GetCurrentCountryAsync();
        return await eventoDao.GetAllEquiposAsync();
    }

    public async Task<IEnumerable<Estadio>> GetAllEstadiosAsync()
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        return await estadioDao.GetAllEstadiosAsync(country);
    }

    public async Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(int idEstadio)
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        return await estadioDao.GetSectoresByEstadioAsync(idEstadio, country);
    }

    public Task<IEnumerable<EventoDetalle>> GetAllEventosDetalladosAsync() =>
        eventoDao.GetAllEventosDetalladosAsync();

    public async Task<IEnumerable<EventoDetalle>> GetManagedEventosAsync()
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        return await eventoDao.GetEventosDetalladosByCountryAsync(country);
    }

    public Task<Dictionary<int, List<SectorHabilitado>>> GetAllSectoresHabilitadosAsync() =>
        eventoDao.GetAllSectoresHabilitadosAsync();

    public async Task<Dictionary<int, List<SectorHabilitado>>> GetManagedSectoresHabilitadosAsync()
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        return await eventoDao.GetSectoresHabilitadosByCountryAsync(country);
    }

    public async Task AgregarEquipoAsync(string nombre)
    {
        await jurisdictionService.GetCurrentCountryAsync();
        try
        {
            await eventoDao.CreateEquipoAsync(nombre);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate") || ex.Message.Contains("unique"))
        {
            throw new InvalidOperationException("Ya existe un equipo con ese nombre.");
        }
    }

    public async Task<int> ProgramarEventoAsync(
        DateTime fechaHora,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio)
    {
        var sectores = sectoresConPrecio.ToList();
        ValidarEvento(fechaHora, idEstadio, idEquipoLocal, idEquipoVisitante, sectores);
        var scope = await jurisdictionService.GetCurrentScopeAsync();
        await EnsureStadiumJurisdictionAsync(idEstadio, scope.CountryName);

        if (await eventoDao.ExisteSuperposicionAsync(idEstadio, fechaHora))
            throw new InvalidOperationException(
                "Ya existe un evento en ese estadio dentro de las 3 horas de la fecha indicada.");

        return await eventoDao.CreateEventoAsync(
            fechaHora, scope.AdministratorId, idEstadio, idEquipoLocal, idEquipoVisitante, sectores);
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
        ValidarEvento(fechaHora, idEstadio, idEquipoLocal, idEquipoVisitante, sectores);
        var country = await jurisdictionService.GetCurrentCountryAsync();
        await EnsureStadiumJurisdictionAsync(idEstadio, country);

        if (await eventoDao.ExisteSuperposicionAsync(idEstadio, fechaHora, idEvento))
            throw new InvalidOperationException(
                "Ya existe un evento en ese estadio dentro de las 3 horas de la fecha indicada.");

        if (!await eventoDao.UpdateEventoAsync(
                idEvento, country, fechaHora, idEstadio,
                idEquipoLocal, idEquipoVisitante, sectores))
        {
            throw new UnauthorizedAccessException(
                "No puede modificar un evento fuera de su país sede.");
        }
    }

    public async Task EliminarEventoAsync(int idEvento)
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        if (!await eventoDao.DeleteEventoAsync(idEvento, country))
        {
            throw new UnauthorizedAccessException(
                "No puede eliminar un evento fuera de su país sede.");
        }
    }

    private async Task EnsureStadiumJurisdictionAsync(int idEstadio, string country)
    {
        if (!await estadioDao.BelongsToCountryAsync(idEstadio, country))
        {
            throw new UnauthorizedAccessException(
                "El estadio seleccionado no pertenece a su país sede.");
        }
    }

    private void ValidarEvento(
        DateTime fechaHora,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        List<(string Sector, decimal Precio)> sectores)
    {
        if (fechaHora < Ahora)
            throw new InvalidOperationException("La fecha y hora del evento no puede ser anterior al momento actual.");
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

    private DateTime Ahora => nowProvider?.Invoke() ?? DateTime.Now;
}
