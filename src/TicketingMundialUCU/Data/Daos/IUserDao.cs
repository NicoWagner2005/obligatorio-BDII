namespace TicketingMundialUCU.Data.Daos;

public interface IUserDao
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
        string role,
        string? paisSedeAsignado);
}
