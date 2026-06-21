using System.Text;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class QrCodeServiceTests
{
    private readonly QrCodeService _service = new();

    [Fact]
    public void GenerateTokenQrDataUri_con_token_vacio_rechaza_la_operacion()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.GenerateTokenQrDataUri(Guid.Empty));

        Assert.Equal("Debe seleccionar un token QR.", exception.Message);
    }

    [Fact]
    public void GenerateTokenQrDataUri_con_token_valido_devuelve_svg_data_uri()
    {
        var dataUri = _service.GenerateTokenQrDataUri(Guid.Parse("3b8f7f3f-bb8d-472d-b3c4-c9b9d8c70b12"));

        Assert.StartsWith("data:image/svg+xml;base64,", dataUri);

        var encodedSvg = dataUri["data:image/svg+xml;base64,".Length..];
        var svg = Encoding.UTF8.GetString(Convert.FromBase64String(encodedSvg));

        Assert.Contains("<svg", svg);
        Assert.Contains("</svg>", svg);
    }
}
