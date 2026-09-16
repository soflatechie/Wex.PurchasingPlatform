namespace Wex.PurchasingPlatform.Models;

public record CreatePurchaseTransactionRequest(
    string Description,
    DateOnly TransactionDate,
    decimal PurchaseAmountUsd);
