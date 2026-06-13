using NSubstitute;
using TicketingMundialUCU.Data.Repositories;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class EventoServiceTests
{
    private readonly IEventoRepository _eventoRepository = Substitute.For<IEventoRepository>();
    private readonly IEstadioRepository _estadioRepository = Substitute.For<IEstadioRepository>();
    private readonly EventoService _service;

    public EventoServiceTests()
    {
        _service = new EventoService(_eventoRepository, _estadioRepository, () => Ahora);
    }

    [Theory]
    [InlineData(0, 1, 2, "Debe seleccionar un estadio.")]
    [InlineData(1, 0, 2, "Debe seleccionar el equipo local.")]
    [InlineData(1, 2, 0, "Debe seleccionar el equipo visitante.")]
    [InlineData(1, 2, 2, "El equipo local y visitante no pueden ser el mismo.")]
    public async Task ProgramarEvento_con_seleccion_invalida_rechaza_la_operacion(
        int idEstadio,
        int idLocal,
        int idVisitante,
        string mensajeEsperado)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProgramarEventoAsync(
                FechaEvento,
                "admin-1",
                idEstadio,
                idLocal,
                idVisitante,
                SectoresValidos));

        Assert.Equal(mensajeEsperado, exception.Message);
        await _eventoRepository.DidNotReceive().ExisteSuperposicionAsync(
            Arg.Any<int>(),
            Arg.Any<DateTime>(),
            Arg.Any<int?>());
    }

    [Fact]
    public async Task ProgramarEvento_sin_sectores_rechaza_la_operacion()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProgramarEventoAsync(
                FechaEvento,
                "admin-1",
                1,
                2,
                3,
                []));

        Assert.Equal("Debe habilitar al menos un sector para el evento.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task ProgramarEvento_con_precio_no_positivo_rechaza_la_operacion(decimal precio)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProgramarEventoAsync(
                FechaEvento,
                "admin-1",
                1,
                2,
                3,
                [("A", precio)]));

        Assert.Equal(
            "El precio de cada sector habilitado debe ser mayor a 0.",
            exception.Message);
    }

    [Fact]
    public async Task ProgramarEvento_con_fecha_hora_anterior_al_momento_actual_rechaza_la_operacion()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProgramarEventoAsync(
                Ahora.AddDays(-1),
                "admin-1",
                1,
                2,
                3,
                SectoresValidos));

        Assert.Equal("La fecha y hora del evento no puede ser anterior al momento actual.", exception.Message);
        await _eventoRepository.DidNotReceive().ExisteSuperposicionAsync(
            Arg.Any<int>(),
            Arg.Any<DateTime>(),
            Arg.Any<int?>());
        await _eventoRepository.DidNotReceive().CreateEventoAsync(
            Arg.Any<DateTime>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<IEnumerable<(string Sector, decimal Precio)>>());
    }

    [Fact]
    public async Task ProgramarEvento_con_hora_de_hoy_ya_pasada_rechaza_la_operacion()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProgramarEventoAsync(
                Ahora.AddMinutes(-1),
                "admin-1",
                1,
                2,
                3,
                SectoresValidos));

        Assert.Equal("La fecha y hora del evento no puede ser anterior al momento actual.", exception.Message);
        await _eventoRepository.DidNotReceive().ExisteSuperposicionAsync(
            Arg.Any<int>(),
            Arg.Any<DateTime>(),
            Arg.Any<int?>());
        await _eventoRepository.DidNotReceive().CreateEventoAsync(
            Arg.Any<DateTime>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<IEnumerable<(string Sector, decimal Precio)>>());
    }

    [Fact]
    public async Task ProgramarEvento_superpuesto_rechaza_la_operacion()
    {
        _eventoRepository.ExisteSuperposicionAsync(1, FechaEvento, null).Returns(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProgramarEventoAsync(
                FechaEvento,
                "admin-1",
                1,
                2,
                3,
                SectoresValidos));

        Assert.Contains("Ya existe un evento", exception.Message);
        await _eventoRepository.DidNotReceive().CreateEventoAsync(
            Arg.Any<DateTime>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<IEnumerable<(string Sector, decimal Precio)>>());
    }

    [Fact]
    public async Task ProgramarEvento_valido_devuelve_el_identificador_creado()
    {
        _eventoRepository.ExisteSuperposicionAsync(1, FechaEvento, null).Returns(false);
        _eventoRepository.CreateEventoAsync(
                FechaEvento,
                "admin-1",
                1,
                2,
                3,
                Arg.Any<IEnumerable<(string Sector, decimal Precio)>>())
            .Returns(25);

        var idEvento = await _service.ProgramarEventoAsync(
            FechaEvento,
            "admin-1",
            1,
            2,
            3,
            SectoresValidos);

        Assert.Equal(25, idEvento);
        await _eventoRepository.Received(1).CreateEventoAsync(
            FechaEvento,
            "admin-1",
            1,
            2,
            3,
            Arg.Is<IEnumerable<(string Sector, decimal Precio)>>(sectores =>
                sectores.SequenceEqual(SectoresValidos)));
    }

    [Fact]
    public async Task ActualizarEvento_excluye_el_evento_actual_al_buscar_superposicion()
    {
        _eventoRepository.ExisteSuperposicionAsync(1, FechaEvento, 9).Returns(false);

        await _service.ActualizarEventoAsync(9, FechaEvento, 1, 2, 3, SectoresValidos);

        await _eventoRepository.Received(1).ExisteSuperposicionAsync(1, FechaEvento, 9);
        await _eventoRepository.Received(1).UpdateEventoAsync(
            9,
            FechaEvento,
            1,
            2,
            3,
            Arg.Is<IEnumerable<(string Sector, decimal Precio)>>(sectores =>
                sectores.SequenceEqual(SectoresValidos)));
    }

    [Fact]
    public async Task ActualizarEvento_con_fecha_hora_anterior_al_momento_actual_rechaza_la_operacion()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ActualizarEventoAsync(
                9,
                Ahora.AddDays(-1),
                1,
                2,
                3,
                SectoresValidos));

        Assert.Equal("La fecha y hora del evento no puede ser anterior al momento actual.", exception.Message);
        await _eventoRepository.DidNotReceive().ExisteSuperposicionAsync(
            Arg.Any<int>(),
            Arg.Any<DateTime>(),
            Arg.Any<int?>());
        await _eventoRepository.DidNotReceive().UpdateEventoAsync(
            Arg.Any<int>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<IEnumerable<(string Sector, decimal Precio)>>());
    }

    [Fact]
    public async Task AgregarEquipo_duplicado_devuelve_un_mensaje_claro()
    {
        _eventoRepository.CreateEquipoAsync("Uruguay")
            .Returns(Task.FromException(new Exception("unique constraint")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AgregarEquipoAsync("Uruguay"));

        Assert.Equal("Ya existe un equipo con ese nombre.", exception.Message);
    }

    private static readonly DateTime Ahora = new(2026, 6, 12, 15, 30, 0);
    private static readonly DateTime FechaEvento = new(2026, 7, 10, 18, 0, 0);

    private static readonly (string Sector, decimal Precio)[] SectoresValidos =
    [
        ("A", 100m),
        ("B", 80m)
    ];
}
