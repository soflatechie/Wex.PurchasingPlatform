namespace Wex.PurchasingPlatform.Models;

public record ConvertedPurchaseTransactionDto(
    int TransactionNumber,
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal PurchaseAmountUsd,
    string Country,
    string CurrencyName,
    decimal ExchangeRate,
    DateOnly RateDate,
    bool IsStale,
    decimal ConvertedAmount);
