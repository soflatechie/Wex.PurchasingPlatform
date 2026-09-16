namespace Wex.PurchasingPlatform.Api.Services.Interfaces;

public interface ICurrencyOptionCacheService
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
