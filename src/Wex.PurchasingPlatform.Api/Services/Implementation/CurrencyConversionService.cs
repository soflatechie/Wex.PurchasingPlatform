using Wex.PurchasingPlatform.Api.Common;
using Wex.PurchasingPlatform.Api.Common.Exceptions;
using Wex.PurchasingPlatform.Api.ExternalServices;
using Wex.PurchasingPlatform.Api.ExternalServices.Interfaces;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.Services.Implementation;

public class CurrencyConversionService(
    PurchaseTransactionRepository transactionRepository,
    CurrencyOptionRepository currencyOptionRepository,
    IExchangeRateProvider exchangeRateProvider,
    ILogger<CurrencyConversionService> logger) : ICurrencyConversionService
{
    private const int StalenessThresholdMonths = 3;

    public async Task<ConvertedPurchaseTransactionDto?> GetConvertedAsync(Guid transactionId, string country, string currencyName, CancellationToken cancellationToken = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
            return null;

        var lookupResult = await exchangeRateProvider.GetLatestRateOnOrBeforeAsync(country, currencyName, transaction.TransactionDate, cancellationToken);
        if (lookupResult is null)
        {
            logger.LogWarning(
                "No exchange rate published for {Country}/{CurrencyName} on or before {TransactionDate}.",
                country, currencyName, transaction.TransactionDate);

            throw new ExchangeRateUnavailableException(
                $"No exchange rate is available for {country}/{currencyName} on or before {transaction.TransactionDate:yyyy-MM-dd}.");
        }

        var rateSelection = SelectRate(lookupResult, transaction.TransactionDate);
        var convertedAmount = MoneyRounding.ToCurrency(transaction.PurchaseAmountUsd * rateSelection.ExchangeRate);

        logger.LogInformation(
            "Converted transaction {TransactionId} to {Country}/{CurrencyName}: rate {ExchangeRate} dated {RateDate}, stale={IsStale}.",
            transactionId, country, currencyName, rateSelection.ExchangeRate, rateSelection.RateDate, rateSelection.IsStale);

        return new ConvertedPurchaseTransactionDto(
            transaction.TransactionNumber,
            transaction.Id,
            transaction.Description,
            transaction.TransactionDate,
            transaction.PurchaseAmountUsd,
            country,
            currencyName,
            rateSelection.ExchangeRate,
            rateSelection.RateDate,
            rateSelection.IsStale,
            convertedAmount);
    }

    public async Task<IReadOnlyList<string>> GetAvailableCountriesAsync(CancellationToken cancellationToken = default)
    {
        var currencyOptions = await currencyOptionRepository.GetAllAsync(cancellationToken);

        return currencyOptions
            .Select(option => option.Country)
            .Distinct()
            .OrderBy(country => country, StringComparer.Ordinal)
            .ToList();
    }

    public async Task<IReadOnlyList<CurrencyOptionDto>> GetAvailableCurrenciesAsync(string country, DateOnly transactionDate, CancellationToken cancellationToken = default)
    {
        var candidates = await currencyOptionRepository.GetByCountryAsync(country, cancellationToken);

        var usableCurrencies = new List<CurrencyOptionDto>();

        foreach (var candidate in candidates)
        {
            var lookupResult = await exchangeRateProvider.GetLatestRateOnOrBeforeAsync(candidate.Country, candidate.CurrencyName, transactionDate, cancellationToken);
            if (lookupResult is not null)
                usableCurrencies.Add(new CurrencyOptionDto(candidate.Country, candidate.CurrencyName));
        }

        return usableCurrencies;
    }

    private static RateSelectionResult SelectRate(ExchangeRateLookupResult lookupResult, DateOnly transactionDate)
    {
        var staleAfterDate = lookupResult.RecordDate.AddMonths(StalenessThresholdMonths);
        var isStale = transactionDate > staleAfterDate;

        return new RateSelectionResult(lookupResult.ExchangeRate, lookupResult.RecordDate, isStale);
    }
}
