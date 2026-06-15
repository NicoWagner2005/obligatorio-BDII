using NSubstitute;
using TicketingMundialUCU.Data.Repositories;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class EstadioServiceTests
{
    private const string Country = "México";

    private readonly IEstadioRepository _repository = Substitute.For<IEstadioRepository>();
    private readonly EstadioService _service;

    public EstadioServiceTests()
    {
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.GetRequiredAdministratorIdAsync().Returns("admin-1");

        var jurisdictionRepository = Substitute.For<IAdministratorJurisdictionRepository>();
        jurisdictionRepository
            .GetCountryForAdministratorAsync("admin-1")
            .Returns(Country);

        var jurisdictionService = new AdministratorJurisdictionService(
            jurisdictionRepository,
            currentUser);
        _service = new EstadioService(_repository, jurisdictionService);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RegistrarEstadio_con_capacidad_no_positiva_rechaza_la_operacion(int capacidad)
    {
        var sectores = CrearSectoresValidos();
        sectores["A"] = capacidad;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegistrarEstadioAsync("Azteca", sectores));

        Assert.Equal("La capacidad de cada sector debe ser mayor a 0.", exception.Message);
    }

    [Fact]
    public async Task RegistrarEstadio_usa_el_pais_del_administrador()
    {
        var sectores = CrearSectoresValidos();

        await _service.RegistrarEstadioAsync("Azteca", sectores);

        await _repository.Received(1).CreateEstadioAsync("Azteca", Country, sectores);
    }

    [Fact]
    public async Task ActualizarEstadio_fuera_de_jurisdiccion_rechaza_la_operacion()
    {
        var sectores = CrearSectoresValidos();
        _repository.UpdateEstadioAsync(10, "Azteca", Country, sectores).Returns(false);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.ActualizarEstadioAsync(10, "Azteca", sectores));

        Assert.Contains("fuera de su país sede", exception.Message);
    }

    [Fact]
    public async Task ActualizarEstadio_con_capacidad_no_positiva_rechaza_la_operacion()
    {
        var sectores = CrearSectoresValidos();
        sectores["D"] = 0;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ActualizarEstadioAsync(10, "Azteca", sectores));

        Assert.Equal("La capacidad de cada sector debe ser mayor a 0.", exception.Message);
        await _repository.DidNotReceiveWithAnyArgs().UpdateEstadioAsync(
            default,
            default!,
            default!,
            default!);
    }

    [Fact]
    public async Task EliminarEstadio_fuera_de_jurisdiccion_rechaza_la_operacion()
    {
        _repository.DeleteEstadioAsync(8, Country).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.EliminarEstadioAsync(8));
    }

    private static Dictionary<string, int> CrearSectoresValidos() => new()
    {
        ["A"] = 100,
        ["B"] = 100,
        ["C"] = 100,
        ["D"] = 100
    };
}
