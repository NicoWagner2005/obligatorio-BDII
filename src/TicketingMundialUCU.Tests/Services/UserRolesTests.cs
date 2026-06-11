using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class UserRolesTests
{
    [Theory]
    [InlineData(UserRoles.General)]
    [InlineData(UserRoles.Funcionario)]
    [InlineData(UserRoles.Administrador)]
    public void IsValid_reconoce_los_roles_soportados(string role)
    {
        Assert.True(UserRoles.IsValid(role));
    }

    [Theory]
    [InlineData("General")]
    [InlineData("admin")]
    [InlineData("")]
    public void IsValid_rechaza_roles_no_soportados(string role)
    {
        Assert.False(UserRoles.IsValid(role));
    }
}
