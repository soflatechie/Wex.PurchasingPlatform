using System.Net.Http.Json;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Desktop.ApiClient;

public class PurchasingApiClient(HttpClient httpClient)
{
    public Task<IReadOnlyList<PurchaseTransactionDto>> GetTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<PurchaseTransactionDto>>("api/purchasetransactions", cancellationToken);
    }

    public async Task<PurchaseTransactionDto> CreateTransactionAsync(CreatePurchaseTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/purchasetransactions", request, cancellationToken);

        return await ReadOrThrowAsync<PurchaseTransactionDto>(response, cancellationToken);
    }

    public async Task<PurchaseTransactionDto> UpdateTransactionAsync(Guid id, UpdatePurchaseTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/purchasetransactions/{id}", request, cancellationToken);

        return await ReadOrThrowAsync<PurchaseTransactionDto>(response, cancellationToken);
    }

    public async Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/purchasetransactions/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw await CreateExceptionAsync(response, cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<string>>("api/countries", cancellationToken);
    }

    public Task<IReadOnlyList<CurrencyOptionDto>> GetCurrenciesAsync(string country, DateOnly transactionDate, CancellationToken cancellationToken = default)
    {
        var requestUri = $"api/currencies?country={Uri.EscapeDataString(country)}&transactionDate={transactionDate:yyyy-MM-dd}";

        return GetAsync<IReadOnlyList<CurrencyOptionDto>>(requestUri, cancellationToken);
    }

    public Task<ConvertedPurchaseTransactionDto> GetConversionAsync(Guid id, string country, string currency, CancellationToken cancellationToken = default)
    {
        var requestUri = $"api/purchasetransactions/{id}/conversion?country={Uri.EscapeDataString(country)}&currency={Uri.EscapeDataString(currency)}";

        return GetAsync<ConvertedPurchaseTransactionDto>(requestUri, cancellationToken);
    }

    private async Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(requestUri, cancellationToken);

        return await ReadOrThrowAsync<TResponse>(response, cancellationToken);
    }

    private static async Task<TResponse> ReadOrThrowAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            throw await CreateExceptionAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

        return payload ?? throw new PurchasingApiException(response.StatusCode, "The API returned an empty response.");
    }

    private static async Task<PurchasingApiException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ProblemDetailsResponse? problemDetails = null;

        try
        {
            problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            // The error body wasn't a ProblemDetails payload; fall back to a generic message below.
        }

        return new PurchasingApiException(response.StatusCode, BuildMessage(problemDetails));
    }

    private static string BuildMessage(ProblemDetailsResponse? problemDetails)
    {
        if (problemDetails is null)
            return "The request failed.";

        if (problemDetails.Errors is { Count: > 0 })
        {
            return string.Join(
                Environment.NewLine,
                problemDetails.Errors.SelectMany(field => field.Value.Select(message => $"{field.Key}: {message}")));
        }

        return problemDetails.Detail ?? problemDetails.Title ?? "The request failed.";
    }
}
