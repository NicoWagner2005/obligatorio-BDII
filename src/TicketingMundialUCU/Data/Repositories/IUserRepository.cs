namespace TicketingMundialUCU.Data.Repositories;

public interface IUserRepository
{
    Task CreateAsync(
        string identityUserId,
        string nroDocumento,
        string tipoDocumento,
        string paisDocumento,
        string paisDireccion,
        string localidad,
        string calle,
        string nroDireccion,
        string codigoPostal,
        string role);
}
