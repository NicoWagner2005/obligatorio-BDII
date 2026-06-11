namespace TicketingMundialUCU.Data.Repositories;

public interface IEstadioRepository
{
    Task<IEnumerable<PaisSede>> GetAllPaisesSedeAsync();
    Task<IEnumerable<Estadio>> GetAllEstadiosAsync();
    Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(int idEstadio);
    Task CreatePaisSedeAsync(string nombre);
    Task<int> CreateEstadioAsync(
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores);
    Task UpdateEstadioAsync(
        int idEstadio,
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores);
    Task DeleteEstadioAsync(int idEstadio);
}
