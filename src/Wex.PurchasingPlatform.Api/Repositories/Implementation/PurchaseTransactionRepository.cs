using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Interfaces;

namespace Wex.PurchasingPlatform.Api.Repositories.Implementation;

public class PurchaseTransactionRepository(AppDbContext context)
    : Repository<PurchaseTransaction>(context), IPurchaseTransactionRepository
{
}
