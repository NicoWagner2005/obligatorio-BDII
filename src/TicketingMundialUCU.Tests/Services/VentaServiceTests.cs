using NSubstitute;
using TicketingMundialUCU.Data.Repositories;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class VentaServiceTests
{
    private readonly IVentaRepository _repository = Substitute.For<IVentaRepository>();
    private readonly VentaService _service;

    public VentaServiceTests()
    {
        _service = new VentaService(_repository);
    }

    [Fact]
    public async Task ComprarEntradas_sin_cantidades_positivas_rechaza_la_compra()
    {
        var items = new List<ItemCarrito>
        {
            new(1, 1, "A", 0),
            new(1, 1, "B", -1)
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ComprarEntradasAsync("usuario-1", items));

        Assert.Equal("Debe seleccionar al menos una entrada.", exception.Message);
        await _repository.DidNotReceive().GetTasaVigenteAsync();
    }

    [Fact]
    public async Task ComprarEntradas_con_mas_de_cinco_boletos_rechaza_la_compra()
    {
        var items = new List<ItemCarrito>
        {
            new(1, 1, "A", 3),
            new(1, 1, "B", 3)
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ComprarEntradasAsync("usuario-1", items));

        Assert.Equal(
            "No se pueden comprar más de 5 entradas por transacción.",
            exception.Message);
    }

    [Fact]
    public async Task ComprarEntradas_valida_filtra_cantidades_y_usa_la_tasa_vigente()
    {
        var tasa = new TasaComision(4, 0.05m, new DateTime(2026, 1, 1));
        _repository.GetTasaVigenteAsync().Returns(tasa);
        _repository.CreateVentaAsync(
                "usuario-1",
                Arg.Any<IEnumerable<ItemCarrito>>(),
                tasa)
            .Returns(37);

        var items = new List<ItemCarrito>
        {
            new(1, 1, "A", 2),
            new(1, 1, "B", 0),
            new(1, 1, "C", 1)
        };

        var idVenta = await _service.ComprarEntradasAsync("usuario-1", items);

        Assert.Equal(37, idVenta);
        await _repository.Received(1).CreateVentaAsync(
            "usuario-1",
            Arg.Is<IEnumerable<ItemCarrito>>(guardados =>
                guardados.SequenceEqual(new[]
                {
                    new ItemCarrito(1, 1, "A", 2),
                    new ItemCarrito(1, 1, "C", 1)
                })),
            tasa);
    }

    [Theory]
    [InlineData("confirmada")]
    [InlineData("paga")]
    public async Task ActualizarEstadoVenta_con_estado_valido_actualiza_la_venta(string estado)
    {
        await _service.ActualizarEstadoVentaAsync(12, estado);

        await _repository.Received(1).UpdateEstadoVentaAsync(12, estado);
    }

    [Theory]
    [InlineData("pendiente")]
    [InlineData("cancelada")]
    [InlineData("")]
    public async Task ActualizarEstadoVenta_con_estado_invalido_rechaza_la_operacion(string estado)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ActualizarEstadoVentaAsync(12, estado));

        Assert.Equal($"El estado '{estado}' no es válido.", exception.Message);
        await _repository.DidNotReceive().UpdateEstadoVentaAsync(
            Arg.Any<int>(),
            Arg.Any<string>());
    }

    // RF-45/RF-46: generar una entrada individual por boleto
    [Fact]
    public async Task ComprarEntradas_genera_una_entrada_individual_por_cada_boleto_solicitado()
    {
        var tasa = new TasaComision(1, 0.10m, new DateTime(2026, 1, 1));
        _repository.GetTasaVigenteAsync().Returns(tasa);
        _repository.CreateVentaAsync("usuario-1", Arg.Any<IEnumerable<ItemCarrito>>(), tasa).Returns(20);

        var items = new List<ItemCarrito>
        {
            new(1, 1, "A", 2),
            new(1, 1, "B", 3),
        };

        await _service.ComprarEntradasAsync("usuario-1", items);

        // El repositorio recibe los ítems con cantidades exactas para generar una fila por boleto
        await _repository.Received(1).CreateVentaAsync(
            "usuario-1",
            Arg.Is<IEnumerable<ItemCarrito>>(its =>
                its.Single(i => i.IdSector == "A").Cantidad == 2 &&
                its.Single(i => i.IdSector == "B").Cantidad == 3 &&
                its.Sum(i => i.Cantidad) == 5),
            tasa);
    }

    // RF: verificar entradas asignadas al usuario
    [Fact]
    public async Task GetEntradasByUsuario_retorna_entradas_asignadas_al_usuario()
    {
        const string userId = "usuario-42";
        var esperadas = new List<EntradaDetalle>
        {
            new(1, 10, 5, new DateTime(2026, 7, 1, 18, 0, 0), "Estadio Central",
                "ARG", "BRA", "A", 1000m, Guid.NewGuid(), "pendiente"),
            new(2, 10, 5, new DateTime(2026, 7, 1, 18, 0, 0), "Estadio Central",
                "ARG", "BRA", "B", 800m,  Guid.NewGuid(), "pendiente"),
        };
        _repository.GetEntradasByUsuarioAsync(userId).Returns(esperadas);

        var resultado = (await _service.GetEntradasByUsuarioAsync(userId)).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal(esperadas, resultado);
        await _repository.Received(1).GetEntradasByUsuarioAsync(userId);
    }

    // RF: asociar entrada a venta
    [Fact]
    public async Task ComprarEntradas_retorna_el_id_de_venta_que_agrupa_las_entradas_compradas()
    {
        var tasa = new TasaComision(1, 0.05m, new DateTime(2026, 1, 1));
        _repository.GetTasaVigenteAsync().Returns(tasa);
        _repository.CreateVentaAsync(Arg.Any<string>(), Arg.Any<IEnumerable<ItemCarrito>>(), tasa).Returns(99);

        var idVenta = await _service.ComprarEntradasAsync("usuario-1", [new(1, 1, "A", 1)]);

        Assert.Equal(99, idVenta);
    }

    // RF: asignación inicial a comprador
    [Fact]
    public async Task ComprarEntradas_asigna_la_venta_al_comprador_en_la_creacion()
    {
        const string compradorId = "comprador-inicial-55";
        var tasa = new TasaComision(1, 0.05m, new DateTime(2026, 1, 1));
        _repository.GetTasaVigenteAsync().Returns(tasa);
        _repository.CreateVentaAsync(compradorId, Arg.Any<IEnumerable<ItemCarrito>>(), tasa).Returns(1);

        await _service.ComprarEntradasAsync(compradorId, [new(1, 1, "A", 1)]);

        await _repository.Received(1).CreateVentaAsync(
            compradorId,
            Arg.Any<IEnumerable<ItemCarrito>>(),
            Arg.Any<TasaComision>());
    }

    // RF: registrar comprador original
    [Fact]
    public async Task GetEntradasByUsuario_retorna_solo_las_entradas_del_comprador_original()
    {
        const string compradorId = "comprador-original-7";
        var entradas = new List<EntradaDetalle>
        {
            new(1, 15, 3, new DateTime(2026, 8, 10, 20, 0, 0), "Estadio Norte",
                "URU", "FRA", "C", 1500m, Guid.NewGuid(), "paga"),
        };
        _repository.GetEntradasByUsuarioAsync(compradorId).Returns(entradas);

        var resultado = (await _service.GetEntradasByUsuarioAsync(compradorId)).ToList();

        Assert.Single(resultado);
        Assert.Equal(15, resultado[0].IdVenta);
        await _repository.Received(1).GetEntradasByUsuarioAsync(compradorId);
    }

    // RF-25: impedir emitir sobre aforo por sector/evento
    [Fact]
    public async Task ComprarEntradas_propaga_error_cuando_supera_el_aforo_del_sector()
    {
        var tasa = new TasaComision(1, 0.05m, new DateTime(2026, 1, 1));
        _repository.GetTasaVigenteAsync().Returns(tasa);
        _repository.CreateVentaAsync(Arg.Any<string>(), Arg.Any<IEnumerable<ItemCarrito>>(), Arg.Any<TasaComision>())
            .Returns(Task.FromException<int>(new InvalidOperationException(
                "Sector A: capacidad insuficiente. Disponibles: 1, solicitadas: 2.")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ComprarEntradasAsync("usuario-1", [new(1, 1, "A", 2)]));

        Assert.Contains("capacidad insuficiente", exception.Message);
    }

    // RF: asociar entrada a sector habilitado
    [Fact]
    public async Task ComprarEntradas_propaga_error_cuando_el_sector_no_esta_habilitado_para_el_evento()
    {
        var tasa = new TasaComision(1, 0.05m, new DateTime(2026, 1, 1));
        _repository.GetTasaVigenteAsync().Returns(tasa);
        _repository.CreateVentaAsync(Arg.Any<string>(), Arg.Any<IEnumerable<ItemCarrito>>(), Arg.Any<TasaComision>())
            .Returns(Task.FromException<int>(new InvalidOperationException(
                "El sector D no está habilitado para este evento.")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ComprarEntradasAsync("usuario-1", [new(1, 1, "D", 1)]));

        Assert.Contains("no está habilitado", exception.Message);
    }
}
