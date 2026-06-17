using NSubstitute;
using TicketingMundialUCU.Data.Daos;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class TransferenciaServiceTests
{
    private readonly ITransferenciaDao _dao = Substitute.For<ITransferenciaDao>();
    private readonly TransferenciaService _service;

    public TransferenciaServiceTests()
    {
        _service = new TransferenciaService(_dao);
    }

    [Fact]
    public async Task SolicitarTransferencia_sin_entrada_rechaza_la_operacion()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SolicitarTransferenciaAsync("usuario-1", Guid.Empty, "otro@ucu.edu.uy"));

        Assert.Equal("Debe seleccionar una entrada.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().CreateSolicitudAsync(default, default!, default!);
    }

    [Fact]
    public async Task SolicitarTransferencia_sin_email_receptor_rechaza_la_operacion()
    {
        var idEntrada = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SolicitarTransferenciaAsync("usuario-1", idEntrada, "   "));

        Assert.Equal("Debe ingresar el email del usuario receptor.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().CreateSolicitudAsync(default, default!, default!);
    }

    [Fact]
    public async Task SolicitarTransferencia_valida_delega_en_el_dao()
    {
        var idEntrada = Guid.NewGuid();
        _dao.CreateSolicitudAsync(idEntrada, "usuario-1", "otro@ucu.edu.uy").Returns(42);

        var idTransferencia = await _service.SolicitarTransferenciaAsync(
            "usuario-1",
            idEntrada,
            "  otro@ucu.edu.uy  ");

        Assert.Equal(42, idTransferencia);
        await _dao.Received(1).CreateSolicitudAsync(
            idEntrada,
            "usuario-1",
            "otro@ucu.edu.uy");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AceptarTransferencia_con_id_invalido_rechaza_la_operacion(int idTransferencia)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AceptarTransferenciaAsync(idTransferencia, "usuario-2"));

        Assert.Equal("La solicitud de transferencia no es válida.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().AcceptAsync(default, default!);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RechazarTransferencia_con_id_invalido_rechaza_la_operacion(int idTransferencia)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RechazarTransferenciaAsync(idTransferencia, "usuario-2"));

        Assert.Equal("La solicitud de transferencia no es válida.", exception.Message);
        await _dao.DidNotReceiveWithAnyArgs().RejectAsync(default, default!);
    }

    [Fact]
    public async Task AceptarTransferencia_valida_delega_en_el_dao()
    {
        await _service.AceptarTransferenciaAsync(12, "usuario-2");

        await _dao.Received(1).AcceptAsync(12, "usuario-2");
    }

    [Fact]
    public async Task RechazarTransferencia_valida_delega_en_el_dao()
    {
        await _service.RechazarTransferenciaAsync(12, "usuario-2");

        await _dao.Received(1).RejectAsync(12, "usuario-2");
    }
}
