using System.Text;
using QRCoder;

namespace TicketingMundialUCU.Services;

public class QrCodeService
{
    public string GenerateTokenQrDataUri(Guid codigoToken)
    {
        if (codigoToken == Guid.Empty)
            throw new InvalidOperationException("Debe seleccionar un token QR.");

        var payload = codigoToken.ToString("D").ToUpperInvariant();

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new SvgQRCode(data);
        var svg = qrCode.GetGraphic(12);
        var encodedSvg = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

        return $"data:image/svg+xml;base64,{encodedSvg}";
    }
}
