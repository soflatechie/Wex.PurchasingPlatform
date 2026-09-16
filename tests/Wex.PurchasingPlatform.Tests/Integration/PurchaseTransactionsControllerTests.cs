using System.Net;
using System.Net.Http.Json;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Tests.Integration;

public class PurchaseTransactionsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_WithValidRequest_ReturnsCreatedWithLocationAndBody()
    {
        var request = new CreatePurchaseTransactionRequest("Office supplies", new DateOnly(2026, 1, 15), 42.50m);

        var response = await _client.PostAsJsonAsync("/api/purchasetransactions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<PurchaseTransactionDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(request.Description, created.Description);
        Assert.Equal(request.PurchaseAmountUsd, created.PurchaseAmountUsd);
    }

    [Fact]
    public async Task Post_WithInvalidRequest_ReturnsBadRequest()
    {
        var request = new CreatePurchaseTransactionRequest(string.Empty, new DateOnly(2026, 1, 15), 42.50m);

        var response = await _client.PostAsJsonAsync("/api/purchasetransactions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenTransactionExists_ReturnsOkWithTransaction()
    {
        var id = await CreateTransactionAsync();

        var response = await _client.GetAsync($"/api/purchasetransactions/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PurchaseTransactionDto>();
        Assert.Equal(id, dto!.Id);
    }

    [Fact]
    public async Task GetById_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/purchasetransactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_IncludesCreatedTransaction()
    {
        var id = await CreateTransactionAsync();

        var response = await _client.GetAsync("/api/purchasetransactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transactions = await response.Content.ReadFromJsonAsync<List<PurchaseTransactionDto>>();
        Assert.Contains(transactions!, transaction => transaction.Id == id);
    }

    [Fact]
    public async Task Put_WhenTransactionExists_ReturnsOkWithUpdatedFields()
    {
        var id = await CreateTransactionAsync();
        var updateRequest = new UpdatePurchaseTransactionRequest("Updated description", new DateOnly(2026, 2, 1), 99.99m);

        var response = await _client.PutAsJsonAsync($"/api/purchasetransactions/{id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PurchaseTransactionDto>();
        Assert.Equal("Updated description", dto!.Description);
        Assert.Equal(99.99m, dto.PurchaseAmountUsd);
    }

    [Fact]
    public async Task Put_WithInvalidRequest_ReturnsBadRequest()
    {
        var id = await CreateTransactionAsync();
        var updateRequest = new UpdatePurchaseTransactionRequest(new string('a', 51), new DateOnly(2026, 2, 1), 99.99m);

        var response = await _client.PutAsJsonAsync($"/api/purchasetransactions/{id}", updateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        var updateRequest = new UpdatePurchaseTransactionRequest("Updated description", new DateOnly(2026, 2, 1), 99.99m);

        var response = await _client.PutAsJsonAsync($"/api/purchasetransactions/{Guid.NewGuid()}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenTransactionExists_ReturnsNoContentThenSubsequentGetReturnsNotFound()
    {
        var id = await CreateTransactionAsync();

        var deleteResponse = await _client.DeleteAsync($"/api/purchasetransactions/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/purchasetransactions/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/purchasetransactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> CreateTransactionAsync(string description = "Integration test transaction", decimal amount = 42.50m)
    {
        var request = new CreatePurchaseTransactionRequest(description, new DateOnly(2026, 1, 15), amount);
        var response = await _client.PostAsJsonAsync("/api/purchasetransactions", request);
        var created = await response.Content.ReadFromJsonAsync<PurchaseTransactionDto>();

        return created!.Id;
    }
}
