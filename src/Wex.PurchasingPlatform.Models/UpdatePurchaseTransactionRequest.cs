namespace Wex.PurchasingPlatform.Models;

public record UpdatePurchaseTransactionRequest(
    string Description,
    DateOnly TransactionDate,
    decimal PurchaseAmountUsd);
