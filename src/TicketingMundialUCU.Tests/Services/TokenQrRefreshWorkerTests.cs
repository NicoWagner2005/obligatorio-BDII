using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TicketingMundialUCU.Data.Daos;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class TokenQrRefreshWorkerTests
{
    [Fact]
    public async Task StartAsync_renueva_tokens_inmediatamente_sin_esperar_intervalo()
    {
        var dao = Substitute.For<ITokenQrDao>();
        var services = new ServiceCollection();
        services.AddScoped(_ => dao);
        services.AddScoped<TokenQrService>();
        await using var provider = services.BuildServiceProvider();
        var worker = new TokenQrRefreshWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Substitute.For<ILogger<TokenQrRefreshWorker>>(),
            TimeProvider.System,
            Options.Create(new TokenQrRefreshOptions
            {
                RefreshInterval = TimeSpan.FromMinutes(5),
            }));

        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await worker.StopAsync(CancellationToken.None);

        await dao.Received(1).RenovarTokensActivosAsync();
    }
}
