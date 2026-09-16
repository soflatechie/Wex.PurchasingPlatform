using Microsoft.AspNetCore.Mvc;
using Moq;
using Wex.PurchasingPlatform.Api.Controllers;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Tests.Controllers;

public class CurrenciesControllerTests
{
    private readonly Mock<ICurrencyConversionService> _conversionService = new();
    private readonly Mock<ICurrencyOptionCacheService> _cacheService = new();
    private readonly CurrenciesController _controller;

    public CurrenciesControllerTests()
    {
        _controller = new CurrenciesController(_conversionService.Object, _cacheService.Object);
    }

    [Fact]
    public async Task GetCountries_ReturnsOkWithCountries()
    {
        var countries = new List<string> { "Canada", "United Kingdom" };

        _conversionService
            .Setup(service => service.GetAvailableCountriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(countries);

        var result = await _controller.GetCountries(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(countries, okResult.Value);
    }

    [Fact]
    public async Task GetAvailableCurrencies_ReturnsOkWithCurrencies()
    {
        var transactionDate = new DateOnly(2026, 1, 15);
        var currencies = new List<CurrencyOptionDto> { new("Canada", "Dollar") };

        _conversionService
            .Setup(service => service.GetAvailableCurrenciesAsync("Canada", transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);

        var result = await _controller.GetAvailableCurrencies("Canada", transactionDate, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(currencies, okResult.Value);
    }

    [Fact]
    public async Task TriggerRefresh_InvokesCacheServiceAndReturnsAccepted()
    {
        var result = await _controller.TriggerRefresh(CancellationToken.None);

        Assert.IsType<AcceptedResult>(result);
        _cacheService.Verify(service => service.RefreshAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
