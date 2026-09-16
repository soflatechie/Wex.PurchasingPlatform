using FluentValidation;
using Mapster;
using Wex.PurchasingPlatform.Api.Common;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Interfaces;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.Services.Implementation;

public class PurchaseTransactionService(
    IPurchaseTransactionRepository repository,
    IValidator<CreatePurchaseTransactionRequest> createValidator,
    IValidator<UpdatePurchaseTransactionRequest> updateValidator) : IPurchaseTransactionService
{
    public async Task<PurchaseTransactionDto> CreateAsync(CreatePurchaseTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var transaction = request.Adapt<PurchaseTransaction>();
        transaction.Id = Guid.CreateVersion7();
        transaction.PurchaseAmountUsd = MoneyRounding.ToCurrency(transaction.PurchaseAmountUsd);
        transaction.CreatedAtUtc = DateTime.UtcNow;

        await repository.AddAsync(transaction, cancellationToken);

        return transaction.Adapt<PurchaseTransactionDto>();
    }

    public async Task<PurchaseTransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await repository.GetByIdAsync(id, cancellationToken);

        return transaction?.Adapt<PurchaseTransactionDto>();
    }

    public async Task<IReadOnlyList<PurchaseTransactionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await repository.GetAllAsync(cancellationToken);

        return transactions.Adapt<List<PurchaseTransactionDto>>();
    }

    public async Task<PurchaseTransactionDto?> UpdateAsync(Guid id, UpdatePurchaseTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var transaction = await repository.GetByIdAsync(id, cancellationToken);

        if (transaction is null)
            return null;

        request.Adapt(transaction);
        transaction.PurchaseAmountUsd = MoneyRounding.ToCurrency(transaction.PurchaseAmountUsd);
        transaction.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(transaction, cancellationToken);

        return transaction.Adapt<PurchaseTransactionDto>();
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }
}
