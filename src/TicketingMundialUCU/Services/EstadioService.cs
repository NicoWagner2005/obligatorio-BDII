using TicketingMundialUCU.Data.Repositories;

namespace TicketingMundialUCU.Services;

public class EstadioService(IEstadioRepository repository)
{
    public Task<IEnumerable<PaisSede>> GetAllPaisesSedeAsync() =>
        repository.GetAllPaisesSedeAsync();

    public Task<IEnumerable<Estadio>> GetAllEstadiosAsync() =>
        repository.GetAllEstadiosAsync();

    public Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(int idEstadio) =>
        repository.GetSectoresByEstadioAsync(idEstadio);

    public async Task AgregarPaisSedeAsync(string nombre)
    {
        try
        {
            await repository.CreatePaisSedeAsync(nombre);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate") || ex.Message.Contains("unique"))
        {
            throw new InvalidOperationException("Ya existe un país sede con ese nombre.");
        }
    }

    public async Task RegistrarEstadioAsync(
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores)
    {
        if (string.IsNullOrWhiteSpace(nombrePaisSede))
            throw new InvalidOperationException("Debe seleccionar un país sede.");
        if (sectores.Values.Any(c => c <= 0))
            throw new InvalidOperationException("La capacidad de cada sector debe ser mayor a 0.");

        await repository.CreateEstadioAsync(nombre, nombrePaisSede, sectores);
    }

    public async Task ActualizarEstadioAsync(
        int idEstadio,
        string nombre,
        string nombrePaisSede,
        Dictionary<string, int> sectores)
    {
        if (sectores.Values.Any(c => c <= 0))
            throw new InvalidOperationException("La capacidad de cada sector debe ser mayor a 0.");

        await repository.UpdateEstadioAsync(idEstadio, nombre, nombrePaisSede, sectores);
    }

    public Task EliminarEstadioAsync(int idEstadio) =>
        repository.DeleteEstadioAsync(idEstadio);
}
