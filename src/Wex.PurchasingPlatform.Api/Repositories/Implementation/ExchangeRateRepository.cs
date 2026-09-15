using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Interfaces;

namespace Wex.PurchasingPlatform.Api.Repositories.Implementation;

public class ExchangeRateRepository(AppDbContext context)
    : Repository<ExchangeRateQuote>(context), IExchangeRateRepository
{
}
