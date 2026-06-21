namespace TicketingMundialUCU.Services;

public class TokenQrRefreshOptions
{
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(30);
}

public class TokenQrRefreshWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TokenQrRefreshWorker> logger,
    TimeProvider timeProvider,
    Microsoft.Extensions.Options.IOptions<TokenQrRefreshOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RenovarAsync(stoppingToken);

        using var timer = new PeriodicTimer(options.Value.RefreshInterval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RenovarAsync(stoppingToken);
        }
    }

    private async Task RenovarAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TokenQrService>();
            var renovados = await service.RenovarTokensActivosAsync();
            logger.LogDebug("Tokens QR renovados: {Cantidad}", renovados);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al renovar tokens QR dinámicos.");
        }
    }
}
