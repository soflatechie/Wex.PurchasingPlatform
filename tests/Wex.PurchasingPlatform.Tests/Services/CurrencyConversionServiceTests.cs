using Microsoft.Extensions.Logging;
using Moq;
using Wex.PurchasingPlatform.Api.Common.Exceptions;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.ExternalServices;
using Wex.PurchasingPlatform.Api.ExternalServices.Interfaces;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Services.Implementation;
using Wex.PurchasingPlatform.Tests.TestDoubles;

namespace Wex.PurchasingPlatform.Tests.Services;

public class CurrencyConversionServiceTests
{
    private const string Country = "Canada";
    private const string CurrencyName = "Dollar";

    private readonly Mock<PurchaseTransactionRepository> _transactionRepository = new(TestDbContextFactory.CreateUnconfigured());
    private readonly Mock<CurrencyOptionRepository> _currencyOptionRepository = new(TestDbContextFactory.CreateUnconfigured());
    private readonly Mock<IExchangeRateProvider> _exchangeRateProvider = new();
    private readonly CurrencyConversionService _service;

    public CurrencyConversionServiceTests()
    {
        _service = new CurrencyConversionService(
            _transactionRepository.Object,
            _currencyOptionRepository.Object,
            _exchangeRateProvider.Object,
            Mock.Of<ILogger<CurrencyConversionService>>());
    }

    [Fact]
    public async Task GetConvertedAsync_WhenTransactionDoesNotExist_ReturnsNull()
    {
        _transactionRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseTransaction?)null);

        var result = await _service.GetConvertedAsync(Guid.NewGuid(), Country, CurrencyName);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetConvertedAsync_WhenNoRateEverPublished_ThrowsExchangeRateUnavailableException()
    {
        var transaction = CreateTransaction(new DateOnly(2026, 1, 15));
        SetupTransaction(transaction);

        _exchangeRateProvider
            .Setup(provider => provider.GetLatestRateOnOrBeforeAsync(Country, CurrencyName, transaction.TransactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExchangeRateLookupResult?)null);

        await Assert.ThrowsAsync<ExchangeRateUnavailableException>(
            () => _service.GetConvertedAsync(transaction.Id, Country, CurrencyName));
    }

    [Fact]
    public async Task GetConvertedAsync_WhenRateDateEqualsTransactionDate_IsNotStale()
    {
        var transactionDate = new DateOnly(2026, 4, 15);
        var transaction = CreateTransaction(transactionDate);
        SetupTransaction(transaction);
        SetupRate(transaction, new ExchangeRateLookupResult(Country, CurrencyName, transactionDate, 1.35m));

        var result = await _service.GetConvertedAsync(transaction.Id, Country, CurrencyName);

        Assert.NotNull(result);
        Assert.False(result!.IsStale);
        Assert.Equal(transactionDate, result.RateDate);
    }

    [Fact]
    public async Task GetConvertedAsync_WhenRateIsExactlyThreeMonthsOld_IsNotStale()
    {
        var rateDate = new DateOnly(2026, 1, 15);
        var transactionDate = rateDate.AddMonths(3);
        var transaction = CreateTransaction(transactionDate);
        SetupTransaction(transaction);
        SetupRate(transaction, new ExchangeRateLookupResult(Country, CurrencyName, rateDate, 1.35m));

        var result = await _service.GetConvertedAsync(transaction.Id, Country, CurrencyName);

        Assert.NotNull(result);
        Assert.False(result!.IsStale);
    }

    [Fact]
    public async Task GetConvertedAsync_WhenRateIsOneDayPastThreeMonths_IsStale()
    {
        var rateDate = new DateOnly(2026, 1, 15);
        var transactionDate = rateDate.AddMonths(3).AddDays(1);
        var transaction = CreateTransaction(transactionDate);
        SetupTransaction(transaction);
        SetupRate(transaction, new ExchangeRateLookupResult(Country, CurrencyName, rateDate, 1.35m));

        var result = await _service.GetConvertedAsync(transaction.Id, Country, CurrencyName);

        Assert.NotNull(result);
        Assert.True(result!.IsStale);
    }

    [Fact]
    public async Task GetConvertedAsync_WhenRateIsFarOlderThanThreeMonths_IsStaleButStillUsed()
    {
        var rateDate = new DateOnly(2020, 1, 15);
        var transactionDate = new DateOnly(2026, 1, 15);
        var transaction = CreateTransaction(transactionDate);
        SetupTransaction(transaction);
        SetupRate(transaction, new ExchangeRateLookupResult(Country, CurrencyName, rateDate, 1.35m));

        var result = await _service.GetConvertedAsync(transaction.Id, Country, CurrencyName);

        Assert.NotNull(result);
        Assert.True(result!.IsStale);
        Assert.Equal(1.35m, result.ExchangeRate);
    }

    [Fact]
    public async Task GetConvertedAsync_RoundsConvertedAmountToNearestCentAwayFromZero()
    {
        var transactionDate = new DateOnly(2026, 4, 15);
        var transaction = CreateTransaction(transactionDate, purchaseAmountUsd: 1.00m);
        SetupTransaction(transaction);
        SetupRate(transaction, new ExchangeRateLookupResult(Country, CurrencyName, transactionDate, 10.005m));

        var result = await _service.GetConvertedAsync(transaction.Id, Country, CurrencyName);

        Assert.NotNull(result);
        Assert.Equal(10.01m, result!.ConvertedAmount);
    }

    [Fact]
    public async Task GetAvailableCountriesAsync_ReturnsDistinctCountriesSortedAlphabetically()
    {
        _currencyOptionRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CurrencyOption { Country = "United Kingdom", CurrencyName = "Pound" },
                new CurrencyOption { Country = "Canada", CurrencyName = "Dollar" },
                new CurrencyOption { Country = "Canada", CurrencyName = "Dollar (legacy)" }
            ]);

        var result = await _service.GetAvailableCountriesAsync();

        Assert.Equal(["Canada", "United Kingdom"], result);
    }

    [Fact]
    public async Task GetAvailableCurrenciesAsync_FiltersToCandidatesWithAUsableRate()
    {
        _currencyOptionRepository
            .Setup(repo => repo.GetByCountryAsync("Germany", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CurrencyOption { Country = "Germany", CurrencyName = "Euro" },
                new CurrencyOption { Country = "Germany", CurrencyName = "Mark" }
            ]);

        var transactionDate = new DateOnly(2026, 1, 15);

        _exchangeRateProvider
            .Setup(provider => provider.GetLatestRateOnOrBeforeAsync("Germany", "Euro", transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRateLookupResult("Germany", "Euro", transactionDate, 0.92m));

        _exchangeRateProvider
            .Setup(provider => provider.GetLatestRateOnOrBeforeAsync("Germany", "Mark", transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExchangeRateLookupResult?)null);

        var result = await _service.GetAvailableCurrenciesAsync("Germany", transactionDate);

        var currency = Assert.Single(result);
        Assert.Equal("Euro", currency.CurrencyName);
    }

    private void SetupTransaction(PurchaseTransaction transaction)
    {
        _transactionRepository
            .Setup(repo => repo.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
    }

    private void SetupRate(PurchaseTransaction transaction, ExchangeRateLookupResult lookupResult)
    {
        _exchangeRateProvider
            .Setup(provider => provider.GetLatestRateOnOrBeforeAsync(Country, CurrencyName, transaction.TransactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lookupResult);
    }

    private static PurchaseTransaction CreateTransaction(DateOnly transactionDate, decimal purchaseAmountUsd = 100.00m)
    {
        return new PurchaseTransaction
        {
            Id = Guid.CreateVersion7(),
            Description = "Office supplies",
            TransactionDate = transactionDate,
            PurchaseAmountUsd = purchaseAmountUsd,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
