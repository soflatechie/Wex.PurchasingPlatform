using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Services.Interfaces;

namespace Wex.PurchasingPlatform.Api.Services.Implementation;

public class CurrencyOptionCacheHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<CurrencyOptionCacheHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var currencyOptionRepository = scope.ServiceProvider.GetRequiredService<CurrencyOptionRepository>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICurrencyOptionCacheService>();

        var existingOptions = await currencyOptionRepository.GetAllAsync(stoppingToken);
        if (existingOptions.Count > 0)
        {
            logger.LogInformation("Currency identity cache already populated with {Count} pairs; skipping startup crawl.", existingOptions.Count);
            return;
        }

        try
        {
            await cacheService.RefreshAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            // An unhandled exception here would crash the whole host (BackgroundService semantics), but
            // Requirement #1's CRUD endpoints must keep working even when Treasury is unreachable at startup.
            logger.LogError(ex, "Currency identity cache startup crawl failed; the cache remains empty until a manual refresh succeeds.");
        }
    }
}
