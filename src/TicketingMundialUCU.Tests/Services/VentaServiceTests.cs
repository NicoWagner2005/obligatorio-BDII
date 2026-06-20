using NSubstitute;
using TicketingMundialUCU.Data.Daos;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class VentaServiceTests
{
    private readonly IVentaDao _dao = Substitute.For<IVentaDao>();
    private readonly IEntradaDao _entradaDao = Substitute.For<IEntradaDao>();
    private readonly VentaService _service;

    public VentaServiceTests()
    {
        _service = new VentaService(_dao, _entradaDao);
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
        await _dao.DidNotReceive().GetTasaVigenteAsync();
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
        _dao.GetTasaVigenteAsync().Returns(tasa);
        _dao.CreateVentaAsync(
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
        await _dao.Received(1).CreateVentaAsync(
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

        await _dao.Received(1).UpdateEstadoVentaAsync(12, estado);
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
        await _dao.DidNotReceive().UpdateEstadoVentaAsync(
            Arg.Any<int>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task GetEntradasByUsuario_delega_en_entrada_dao()
    {
        var entradas = new[]
        {
            new EntradaDetalle(
                Guid.NewGuid(),
                1,
                1,
                10,
                new DateTime(2026, 6, 20, 18, 0, 0),
                "Estadio",
                "Local",
                "Visitante",
                "A",
                100m,
                "paga",
                false)
        };
        _entradaDao.GetEntradasByUsuarioAsync("usuario-1").Returns(entradas);

        var resultado = await _service.GetEntradasByUsuarioAsync("usuario-1");

        Assert.Same(entradas, resultado);
        await _entradaDao.Received(1).GetEntradasByUsuarioAsync("usuario-1");
    }

    [Fact]
    public async Task GetDetallesByVenta_delega_en_entrada_dao()
    {
        var entradas = Array.Empty<EntradaDetalle>();
        _entradaDao.GetDetallesByVentaAsync(37).Returns(entradas);

        var resultado = await _service.GetDetallesByVentaAsync(37);

        Assert.Same(entradas, resultado);
        await _entradaDao.Received(1).GetDetallesByVentaAsync(37);
    }
}
