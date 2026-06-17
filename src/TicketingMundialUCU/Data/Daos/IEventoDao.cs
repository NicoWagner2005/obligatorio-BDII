namespace TicketingMundialUCU.Data.Daos;

public interface IEventoDao
{
    Task<IEnumerable<Equipo>> GetAllEquiposAsync();
    Task CreateEquipoAsync(string nombre);
    Task<IEnumerable<EventoDetalle>> GetAllEventosDetalladosAsync();
    Task<IEnumerable<EventoDetalle>> GetEventosDetalladosByCountryAsync(string nombrePaisSede);
    Task<Dictionary<int, List<SectorHabilitado>>> GetAllSectoresHabilitadosAsync();
    Task<Dictionary<int, List<SectorHabilitado>>> GetSectoresHabilitadosByCountryAsync(
        string nombrePaisSede);
    Task<bool> ExisteSuperposicionAsync(
        int idEstadio,
        DateTime fechaHora,
        int? idEventoExcluir = null);
    Task<int> CreateEventoAsync(
        DateTime fechaHora,
        string idAdministrador,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio);
    Task<bool> UpdateEventoAsync(
        int idEvento,
        string nombrePaisSede,
        DateTime fechaHora,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio);
    Task<bool> DeleteEventoAsync(int idEvento, string nombrePaisSede);
}
