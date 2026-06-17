using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public class EstadioService(
    IEstadioDao dao,
    AdministratorJurisdictionService jurisdictionService)
{
    public Task<string> GetCurrentCountryAsync() =>
        jurisdictionService.GetCurrentCountryAsync();

    public async Task<IEnumerable<Estadio>> GetAllEstadiosAsync()
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        return await dao.GetAllEstadiosAsync(country);
    }

    public async Task<IEnumerable<Sector>> GetSectoresByEstadioAsync(int idEstadio)
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        return await dao.GetSectoresByEstadioAsync(idEstadio, country);
    }

    public async Task RegistrarEstadioAsync(
        string nombre,
        Dictionary<string, int> sectores)
    {
        if (sectores.Values.Any(c => c <= 0))
            throw new InvalidOperationException("La capacidad de cada sector debe ser mayor a 0.");

        var country = await jurisdictionService.GetCurrentCountryAsync();
        await dao.CreateEstadioAsync(nombre, country, sectores);
    }

    public async Task ActualizarEstadioAsync(
        int idEstadio,
        string nombre,
        Dictionary<string, int> sectores)
    {
        if (sectores.Values.Any(c => c <= 0))
            throw new InvalidOperationException("La capacidad de cada sector debe ser mayor a 0.");

        var country = await jurisdictionService.GetCurrentCountryAsync();
        if (!await dao.UpdateEstadioAsync(idEstadio, nombre, country, sectores))
        {
            throw new UnauthorizedAccessException(
                "No puede modificar un estadio fuera de su país sede.");
        }
    }

    public async Task EliminarEstadioAsync(int idEstadio)
    {
        var country = await jurisdictionService.GetCurrentCountryAsync();
        if (!await dao.DeleteEstadioAsync(idEstadio, country))
        {
            throw new UnauthorizedAccessException(
                "No puede eliminar un estadio fuera de su país sede.");
        }
    }
}
