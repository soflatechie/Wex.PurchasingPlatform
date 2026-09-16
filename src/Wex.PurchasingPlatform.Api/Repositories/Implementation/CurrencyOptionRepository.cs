using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Interfaces;

namespace Wex.PurchasingPlatform.Api.Repositories.Implementation;

public class CurrencyOptionRepository(AppDbContext context)
    : Repository<CurrencyOption>(context), ICurrencyOptionRepository
{
    public async Task<IReadOnlyList<CurrencyOption>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CurrencyOption>()
            .Where(option => option.Country == country)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAllAsync(IReadOnlyList<CurrencyOption> options, CancellationToken cancellationToken = default)
    {
        await Context.Set<CurrencyOption>().ExecuteDeleteAsync(cancellationToken);

        await Context.Set<CurrencyOption>().AddRangeAsync(options, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
