using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TicketingMundialUCU.Services;

public sealed class CurrentUserContext(AuthenticationStateProvider authenticationStateProvider)
    : ICurrentUserContext
{
    public async Task<string> GetRequiredAdministratorIdAsync()
    {
        var principal = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null || !principal.IsInRole(UserRoles.Administrador))
        {
            throw new UnauthorizedAccessException(
                "Se requiere un administrador autenticado para realizar esta operación.");
        }

        return userId;
    }

    public async Task<string> GetRequiredFuncionarioIdAsync()
    {
        var principal = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null || !principal.IsInRole(UserRoles.Funcionario))
        {
            throw new UnauthorizedAccessException(
                "Se requiere un funcionario autenticado para realizar esta operación.");
        }

        return userId;
    }
}
