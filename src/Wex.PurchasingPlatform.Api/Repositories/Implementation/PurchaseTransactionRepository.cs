using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Entities;

namespace Wex.PurchasingPlatform.Api.Repositories.Implementation;

public class PurchaseTransactionRepository(AppDbContext context) : Repository<PurchaseTransaction>(context)
{
    // The base class's GetByIdAsync/DeleteAsync look up by the database primary key (TransactionNumber),
    // but every caller in this app identifies a transaction by its Guid Id instead - see AppDbContext.
    public override async Task<PurchaseTransaction?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        var transactionId = (Guid)id;

        return await Context.Set<PurchaseTransaction>()
            .FirstOrDefaultAsync(transaction => transaction.Id == transactionId, cancellationToken);
    }

    public override async Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(id, cancellationToken);
        if (transaction is null)
            return false;

        Context.Set<PurchaseTransaction>().Remove(transaction);
        await Context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
