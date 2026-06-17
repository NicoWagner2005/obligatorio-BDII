namespace TicketingMundialUCU.Data.Daos;

public interface IEstadioDao
{
    Task<IEnumerable<Estadio>> GetAllEstadiosAsync(string nombrePaisSede);
    Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(int idEstadio, string nombrePaisSede);
    Task<bool> BelongsToCountryAsync(int idEstadio, string nombrePaisSede);
    Task<int> CreateEstadioAsync(
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores);
    Task<bool> UpdateEstadioAsync(
        int idEstadio,
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores);
    Task<bool> DeleteEstadioAsync(int idEstadio, string nombrePaisSede);
}
