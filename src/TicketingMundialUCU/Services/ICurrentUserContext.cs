namespace TicketingMundialUCU.Services;

public interface ICurrentUserContext
{
    Task<string> GetRequiredAdministratorIdAsync();
}
