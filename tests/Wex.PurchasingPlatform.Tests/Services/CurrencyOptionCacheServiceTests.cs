using Microsoft.Extensions.Logging;
using Moq;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.ExternalServices.Interfaces;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Services.Implementation;
using Wex.PurchasingPlatform.Models;
using Wex.PurchasingPlatform.Tests.TestDoubles;

namespace Wex.PurchasingPlatform.Tests.Services;

public class CurrencyOptionCacheServiceTests
{
    private readonly Mock<CurrencyOptionRepository> _currencyOptionRepository = new(TestDbContextFactory.CreateUnconfigured());
    private readonly Mock<IExchangeRateProvider> _exchangeRateProvider = new();
    private readonly CurrencyOptionCacheService _service;

    public CurrencyOptionCacheServiceTests()
    {
        _service = new CurrencyOptionCacheService(
            _currencyOptionRepository.Object,
            _exchangeRateProvider.Object,
            Mock.Of<ILogger<CurrencyOptionCacheService>>());
    }

    [Fact]
    public async Task RefreshAsync_ReplacesCurrencyOptionsWithProviderResults()
    {
        var options = new List<CurrencyOptionDto>
        {
            new("Canada", "Dollar"),
            new("United Kingdom", "Pound")
        };

        _exchangeRateProvider
            .Setup(provider => provider.FetchAllCurrencyOptionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(options);

        await _service.RefreshAsync();

        _currencyOptionRepository.Verify(
            repo => repo.ReplaceAllAsync(
                It.Is<IReadOnlyList<CurrencyOption>>(entities =>
                    entities.Count == 2 &&
                    entities.Any(entity => entity.Country == "Canada" && entity.CurrencyName == "Dollar") &&
                    entities.Any(entity => entity.Country == "United Kingdom" && entity.CurrencyName == "Pound")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenProviderReturnsNoOptions_ReplacesWithEmptyList()
    {
        _exchangeRateProvider
            .Setup(provider => provider.FetchAllCurrencyOptionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _service.RefreshAsync();

        _currencyOptionRepository.Verify(
            repo => repo.ReplaceAllAsync(
                It.Is<IReadOnlyList<CurrencyOption>>(entities => entities.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
