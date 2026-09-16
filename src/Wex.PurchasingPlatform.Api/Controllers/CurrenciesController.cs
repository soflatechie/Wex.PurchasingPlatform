using Microsoft.AspNetCore.Mvc;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.Controllers;

[ApiController]
public class CurrenciesController(
    ICurrencyConversionService conversionService,
    ICurrencyOptionCacheService cacheService) : ControllerBase
{
    [HttpGet("api/countries")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCountries(CancellationToken cancellationToken)
    {
        var countries = await conversionService.GetAvailableCountriesAsync(cancellationToken);

        return Ok(countries);
    }

    [HttpGet("api/currencies")]
    public async Task<ActionResult<IReadOnlyList<CurrencyOptionDto>>> GetAvailableCurrencies(
        [FromQuery] string country,
        [FromQuery] DateOnly transactionDate,
        CancellationToken cancellationToken)
    {
        var currencies = await conversionService.GetAvailableCurrenciesAsync(country, transactionDate, cancellationToken);

        return Ok(currencies);
    }

    [HttpPost("api/currencies/refresh")]
    public async Task<IActionResult> TriggerRefresh(CancellationToken cancellationToken)
    {
        await cacheService.RefreshAsync(cancellationToken);

        return Accepted();
    }
}
