using NSubstitute;
using TicketingMundialUCU.Data.Repositories;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class EstadioServiceTests
{
    private readonly IEstadioRepository _repository = Substitute.For<IEstadioRepository>();
    private readonly EstadioService _service;

    public EstadioServiceTests()
    {
        _service = new EstadioService(_repository);
    }

    [Fact]
    public async Task RegistrarEstadio_sin_pais_rechaza_la_operacion()
    {
        var sectores = CrearSectoresValidos();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegistrarEstadioAsync("Centenario", " ", sectores));

        Assert.Equal("Debe seleccionar un país sede.", exception.Message);
        await _repository.DidNotReceive().CreateEstadioAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Dictionary<string, int>>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RegistrarEstadio_con_capacidad_no_positiva_rechaza_la_operacion(int capacidad)
    {
        var sectores = CrearSectoresValidos();
        sectores["A"] = capacidad;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegistrarEstadioAsync("Centenario", "Uruguay", sectores));

        Assert.Equal("La capacidad de cada sector debe ser mayor a 0.", exception.Message);
    }

    [Fact]
    public async Task RegistrarEstadio_con_datos_validos_guarda_el_estadio()
    {
        var sectores = CrearSectoresValidos();

        await _service.RegistrarEstadioAsync("Centenario", "Uruguay", sectores);

        await _repository.Received(1).CreateEstadioAsync("Centenario", "Uruguay", sectores);
    }

    [Fact]
    public async Task ActualizarEstadio_con_capacidad_no_positiva_rechaza_la_operacion()
    {
        var sectores = CrearSectoresValidos();
        sectores["D"] = 0;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ActualizarEstadioAsync(10, "Centenario", "Uruguay", sectores));

        Assert.Equal("La capacidad de cada sector debe ser mayor a 0.", exception.Message);
        await _repository.DidNotReceive().UpdateEstadioAsync(
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Dictionary<string, int>>());
    }

    [Fact]
    public async Task AgregarPais_duplicado_devuelve_un_mensaje_claro()
    {
        _repository.CreatePaisSedeAsync("Uruguay")
            .Returns(Task.FromException(new Exception("duplicate key value")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AgregarPaisSedeAsync("Uruguay"));

        Assert.Equal("Ya existe un país sede con ese nombre.", exception.Message);
    }

    [Fact]
    public async Task EliminarEstadio_delega_la_eliminacion_al_repositorio()
    {
        await _service.EliminarEstadioAsync(8);

        await _repository.Received(1).DeleteEstadioAsync(8);
    }

    private static Dictionary<string, int> CrearSectoresValidos() => new()
    {
        ["A"] = 100,
        ["B"] = 100,
        ["C"] = 100,
        ["D"] = 100
    };
}
