namespace Wex.PurchasingPlatform.Models;

public record PurchaseTransactionDto(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal PurchaseAmountUsd);
