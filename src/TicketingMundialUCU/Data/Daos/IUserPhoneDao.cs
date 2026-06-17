namespace TicketingMundialUCU.Data.Daos;

public interface IUserPhoneDao
{
    Task AddAsync(string userId, string telefono);
    Task ReplaceAsync(string userId, string? telefono);
    Task ReplaceAllAsync(string userId, IEnumerable<string?> telefonos);
    Task<IEnumerable<string>> GetByUserIdAsync(string userId);
    Task DeleteAsync(string userId, string telefono);
}
