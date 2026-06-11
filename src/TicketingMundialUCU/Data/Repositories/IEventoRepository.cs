namespace TicketingMundialUCU.Data.Repositories;

public interface IEventoRepository
{
    Task<IEnumerable<Equipo>> GetAllEquiposAsync();
    Task CreateEquipoAsync(string nombre);
    Task<IEnumerable<EventoDetalle>> GetAllEventosDetalladosAsync();
    Task<Dictionary<int, List<SectorHabilitado>>> GetAllSectoresHabilitadosAsync();
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
    Task UpdateEventoAsync(
        int idEvento,
        DateTime fechaHora,
        int idEstadio,
        int idEquipoLocal,
        int idEquipoVisitante,
        IEnumerable<(string Sector, decimal Precio)> sectoresConPrecio);
    Task DeleteEventoAsync(int idEvento);
}
