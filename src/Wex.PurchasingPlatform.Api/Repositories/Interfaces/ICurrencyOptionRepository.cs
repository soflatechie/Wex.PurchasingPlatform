using Wex.PurchasingPlatform.Api.Entities;

namespace Wex.PurchasingPlatform.Api.Repositories.Interfaces;

public interface ICurrencyOptionRepository : IRepository<CurrencyOption>
{
    Task<IReadOnlyList<CurrencyOption>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);

    Task ReplaceAllAsync(IReadOnlyList<CurrencyOption> options, CancellationToken cancellationToken = default);
}
