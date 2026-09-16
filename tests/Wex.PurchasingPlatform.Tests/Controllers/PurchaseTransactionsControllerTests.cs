using Microsoft.AspNetCore.Mvc;
using Moq;
using Wex.PurchasingPlatform.Api.Controllers;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Tests.Controllers;

public class PurchaseTransactionsControllerTests
{
    private readonly Mock<IPurchaseTransactionService> _transactionService = new();
    private readonly Mock<ICurrencyConversionService> _conversionService = new();
    private readonly PurchaseTransactionsController _controller;

    public PurchaseTransactionsControllerTests()
    {
        _controller = new PurchaseTransactionsController(_transactionService.Object, _conversionService.Object);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWithTransaction()
    {
        var request = new CreatePurchaseTransactionRequest("Office supplies", new DateOnly(2026, 1, 15), 42.50m);
        var created = new PurchaseTransactionDto(1, Guid.NewGuid(), request.Description, request.TransactionDate, request.PurchaseAmountUsd);

        _transactionService
            .Setup(service => service.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _controller.Create(request, CancellationToken.None);

        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(PurchaseTransactionsController.GetById), createdAtAction.ActionName);
        Assert.Equal(created, createdAtAction.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTransactions()
    {
        var transactions = new List<PurchaseTransactionDto>
        {
            new(1, Guid.NewGuid(), "Office supplies", new DateOnly(2026, 1, 15), 42.50m)
        };

        _transactionService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(transactions, okResult.Value);
    }

    [Fact]
    public async Task GetById_WhenTransactionExists_ReturnsOkWithTransaction()
    {
        var transaction = new PurchaseTransactionDto(1, Guid.NewGuid(), "Office supplies", new DateOnly(2026, 1, 15), 42.50m);

        _transactionService
            .Setup(service => service.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _controller.GetById(transaction.Id, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(transaction, okResult.Value);
    }

    [Fact]
    public async Task GetById_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        _transactionService
            .Setup(service => service.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseTransactionDto?)null);

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_WhenTransactionExists_ReturnsOkWithUpdatedTransaction()
    {
        var id = Guid.NewGuid();
        var request = new UpdatePurchaseTransactionRequest("Updated description", new DateOnly(2026, 2, 1), 99.99m);
        var updated = new PurchaseTransactionDto(1, id, request.Description, request.TransactionDate, request.PurchaseAmountUsd);

        _transactionService
            .Setup(service => service.UpdateAsync(id, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _controller.Update(id, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updated, okResult.Value);
    }

    [Fact]
    public async Task Update_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        var request = new UpdatePurchaseTransactionRequest("Updated description", new DateOnly(2026, 2, 1), 99.99m);

        _transactionService
            .Setup(service => service.UpdateAsync(It.IsAny<Guid>(), request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseTransactionDto?)null);

        var result = await _controller.Update(Guid.NewGuid(), request, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WhenTransactionExists_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        _transactionService
            .Setup(service => service.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        _transactionService
            .Setup(service => service.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetConversion_WhenTransactionExists_ReturnsOkWithConvertedTransaction()
    {
        var id = Guid.NewGuid();
        var converted = new ConvertedPurchaseTransactionDto(
            1, id, "Office supplies", new DateOnly(2026, 1, 15), 100.00m,
            "Canada", "Dollar", 1.35m, new DateOnly(2026, 1, 15), false, 135.00m);

        _conversionService
            .Setup(service => service.GetConvertedAsync(id, "Canada", "Dollar", It.IsAny<CancellationToken>()))
            .ReturnsAsync(converted);

        var result = await _controller.GetConversion(id, "Canada", "Dollar", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(converted, okResult.Value);
    }

    [Fact]
    public async Task GetConversion_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        _conversionService
            .Setup(service => service.GetConvertedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConvertedPurchaseTransactionDto?)null);

        var result = await _controller.GetConversion(Guid.NewGuid(), "Canada", "Dollar", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
