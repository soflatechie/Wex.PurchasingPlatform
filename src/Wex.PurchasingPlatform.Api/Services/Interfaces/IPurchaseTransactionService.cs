using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.Services.Interfaces;

public interface IPurchaseTransactionService
{
    Task<PurchaseTransactionDto> CreateAsync(CreatePurchaseTransactionRequest request, CancellationToken cancellationToken = default);

    Task<PurchaseTransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseTransactionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PurchaseTransactionDto?> UpdateAsync(Guid id, UpdatePurchaseTransactionRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
