using NSubstitute;
using TicketingMundialUCU.Data.Daos;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class FuncionarioServiceTests
{
    private readonly IFuncionarioDao _dao = Substitute.For<IFuncionarioDao>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly FuncionarioService _service;

    public FuncionarioServiceTests()
    {
        _currentUser.GetRequiredFuncionarioIdAsync().Returns("funcionario-1");
        _service = new FuncionarioService(_dao, _currentUser);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task RegistrarDispositivo_sin_id_rechaza_la_operacion(string idDispositivo)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegistrarDispositivoAsync(idDispositivo, "funcionario-1"));

        Assert.Equal("El ID del dispositivo es obligatorio.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().CreateDispositivoAsync(default!, default!);
    }

    [Fact]
    public async Task RegistrarDispositivo_valido_recorta_el_id_y_delega_en_el_dao()
    {
        await _service.RegistrarDispositivoAsync("  SCANNER-009  ", "funcionario-1");

        await _dao.Received(1).CreateDispositivoAsync("SCANNER-009", "funcionario-1");
    }

    [Fact]
    public async Task EliminarDispositivo_valido_delega_en_el_dao_con_id_textual()
    {
        _dao.DeleteDispositivoAsync("SCANNER-010").Returns(true);

        await _service.EliminarDispositivoAsync("SCANNER-010");

        await _dao.Received(1).DeleteDispositivoAsync("SCANNER-010");
    }

    [Fact]
    public async Task EliminarDispositivo_inexistente_rechaza_la_operacion()
    {
        _dao.DeleteDispositivoAsync("SCANNER-999").Returns(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.EliminarDispositivoAsync("SCANNER-999"));

        Assert.Equal("No se encontró el dispositivo.", exception.Message);
    }

    [Fact]
    public async Task ValidarEntrada_con_dispositivo_no_autorizado_rechaza_la_operacion()
    {
        var codigoToken = Guid.NewGuid();
        _dao.IsDispositivoDelFuncionarioAsync("SCANNER-011", "funcionario-1").Returns(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ValidarEntradaAsync(codigoToken, "SCANNER-011"));

        Assert.Equal("El dispositivo no está autorizado para este funcionario.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().GetEntradaParaValidarAsync(default);
        await _dao.DidNotReceiveWithAnyArgs().ValidarEntradaAsync(default, default!, default!);
    }

    [Fact]
    public async Task ValidarEntrada_con_token_inexistente_o_expirado_rechaza_la_operacion()
    {
        var codigoToken = Guid.NewGuid();
        _dao.IsDispositivoDelFuncionarioAsync("SCANNER-012", "funcionario-1").Returns(true);
        _dao.GetEntradaParaValidarAsync(codigoToken).Returns((EntradaValidacionInfo?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ValidarEntradaAsync(codigoToken, "SCANNER-012"));

        Assert.Equal("No se encontró un token QR válido.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().ExisteAsignacionAsync(default!, default, default, default!);
        await _dao.DidNotReceiveWithAnyArgs().ValidarEntradaAsync(default, default!, default!);
    }

    [Fact]
    public async Task ValidarEntrada_con_entrada_ya_validada_rechaza_la_operacion()
    {
        var codigoToken = Guid.NewGuid();
        var idEntrada = Guid.NewGuid();
        _dao.IsDispositivoDelFuncionarioAsync("SCANNER-012", "funcionario-1").Returns(true);
        _dao.GetEntradaParaValidarAsync(codigoToken)
            .Returns(new EntradaValidacionInfo(codigoToken, idEntrada, true, 7, 3, "A", "paga"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ValidarEntradaAsync(codigoToken, "SCANNER-012"));

        Assert.Equal("La entrada ya fue validada anteriormente.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().ExisteAsignacionAsync(default!, default, default, default!);
        await _dao.DidNotReceiveWithAnyArgs().ValidarEntradaAsync(default, default!, default!);
    }

    [Fact]
    public async Task ValidarEntrada_sin_asignacion_al_sector_rechaza_la_operacion()
    {
        var codigoToken = Guid.NewGuid();
        var idEntrada = Guid.NewGuid();
        _dao.IsDispositivoDelFuncionarioAsync("SCANNER-012", "funcionario-1").Returns(true);
        _dao.GetEntradaParaValidarAsync(codigoToken)
            .Returns(new EntradaValidacionInfo(codigoToken, idEntrada, false, 7, 3, "A", "paga"));
        _dao.ExisteAsignacionAsync("funcionario-1", 7, 3, "A").Returns(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ValidarEntradaAsync(codigoToken, "SCANNER-012"));

        Assert.Equal("No tenés asignación para el sector de esta entrada en este evento.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().ValidarEntradaAsync(default, default!, default!);
    }

    [Theory]
    [InlineData("pendiente")]
    [InlineData("confirmada")]
    public async Task ValidarEntrada_con_venta_no_paga_rechaza_la_operacion(string estadoVenta)
    {
        var codigoToken = Guid.NewGuid();
        var idEntrada = Guid.NewGuid();
        _dao.IsDispositivoDelFuncionarioAsync("SCANNER-012", "funcionario-1").Returns(true);
        _dao.GetEntradaParaValidarAsync(codigoToken)
            .Returns(new EntradaValidacionInfo(codigoToken, idEntrada, false, 7, 3, "A", estadoVenta));
        _dao.ExisteAsignacionAsync("funcionario-1", 7, 3, "A").Returns(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ValidarEntradaAsync(codigoToken, "SCANNER-012"));

        Assert.Equal("La venta de esta entrada no está marcada como paga.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().ValidarEntradaAsync(default, default!, default!);
    }

    [Fact]
    public async Task ValidarEntrada_valida_delega_en_el_dao_con_id_dispositivo_textual()
    {
        var codigoToken = Guid.NewGuid();
        var idEntrada = Guid.NewGuid();
        _dao.IsDispositivoDelFuncionarioAsync("SCANNER-012", "funcionario-1").Returns(true);
        _dao.GetEntradaParaValidarAsync(codigoToken)
            .Returns(new EntradaValidacionInfo(codigoToken, idEntrada, false, 7, 3, "A", "paga"));
        _dao.ExisteAsignacionAsync("funcionario-1", 7, 3, "A").Returns(true);

        await _service.ValidarEntradaAsync(codigoToken, "SCANNER-012");

        await _dao.Received(1).ValidarEntradaAsync(
            codigoToken,
            "funcionario-1",
            "SCANNER-012");
    }
}
