using NSubstitute;
using TicketingMundialUCU.Data.Daos;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class TokenQrServiceTests
{
    private readonly ITokenQrDao _dao = Substitute.For<ITokenQrDao>();
    private readonly TokenQrService _service;

    public TokenQrServiceTests()
    {
        _service = new TokenQrService(_dao);
    }

    [Fact]
    public async Task RenovarTokensActivos_delega_en_el_dao()
    {
        _dao.RenovarTokensActivosAsync().Returns(3);

        var renovados = await _service.RenovarTokensActivosAsync();

        Assert.Equal(3, renovados);
        await _dao.Received(1).RenovarTokensActivosAsync();
    }

    [Fact]
    public async Task GetTokenActivo_con_entrada_vacia_rechaza_la_operacion()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetTokenActivoByEntradaAsync("usuario-1", Guid.Empty));

        Assert.Equal("Debe seleccionar una entrada.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().GetTokenActivoByEntradaAsync(default!, default);
    }

    [Fact]
    public async Task GetTokenActivo_valido_delega_en_el_dao()
    {
        var idEntrada = Guid.NewGuid();
        var token = new TokenQrActivo(
            idEntrada,
            Guid.NewGuid(),
            new DateTime(2026, 6, 20, 12, 0, 30));
        _dao.GetTokenActivoByEntradaAsync("usuario-1", idEntrada).Returns(token);

        var resultado = await _service.GetTokenActivoByEntradaAsync("usuario-1", idEntrada);

        Assert.Same(token, resultado);
        await _dao.Received(1).GetTokenActivoByEntradaAsync("usuario-1", idEntrada);
    }
}
