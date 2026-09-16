namespace Wex.PurchasingPlatform.Models;

public record PurchaseTransactionDto(
    int TransactionNumber,
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal PurchaseAmountUsd);
