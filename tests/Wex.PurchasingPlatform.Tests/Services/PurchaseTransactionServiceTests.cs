using FluentValidation;
using FluentValidation.Results;
using Moq;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Interfaces;
using Wex.PurchasingPlatform.Api.Services.Implementation;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Tests.Services;

public class PurchaseTransactionServiceTests
{
    private readonly Mock<IPurchaseTransactionRepository> _repository = new();
    private readonly Mock<IValidator<CreatePurchaseTransactionRequest>> _createValidator = new();
    private readonly Mock<IValidator<UpdatePurchaseTransactionRequest>> _updateValidator = new();
    private readonly PurchaseTransactionService _service;

    public PurchaseTransactionServiceTests()
    {
        _createValidator
            .Setup(validator => validator.ValidateAsync(It.IsAny<CreatePurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _updateValidator
            .Setup(validator => validator.ValidateAsync(It.IsAny<UpdatePurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _service = new PurchaseTransactionService(_repository.Object, _createValidator.Object, _updateValidator.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_PersistsAndReturnsMappedDto()
    {
        var request = new CreatePurchaseTransactionRequest("Office supplies", new DateOnly(2026, 1, 15), 42.50m);

        var result = await _service.CreateAsync(request);

        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.TransactionDate, result.TransactionDate);
        Assert.Equal(request.PurchaseAmountUsd, result.PurchaseAmountUsd);
        Assert.NotEqual(Guid.Empty, result.Id);
        _repository.Verify(repo => repo.AddAsync(It.IsAny<PurchaseTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestFailsValidation_DoesNotCallRepositoryAndThrows()
    {
        var request = new CreatePurchaseTransactionRequest(string.Empty, new DateOnly(2026, 1, 15), 42.50m);

        _createValidator.Reset();
        _createValidator
            .Setup(validator => validator.ValidateAsync(It.IsAny<CreatePurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Description", "Description is required.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request));

        _repository.Verify(repo => repo.AddAsync(It.IsAny<PurchaseTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RoundsPurchaseAmountToTheNearestCent()
    {
        var request = new CreatePurchaseTransactionRequest("Office supplies", new DateOnly(2026, 1, 15), 42.505m);

        var result = await _service.CreateAsync(request);

        Assert.Equal(42.51m, result.PurchaseAmountUsd);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionDoesNotExist_ReturnsNull()
    {
        _repository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseTransaction?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionExists_ReturnsMappedDto()
    {
        var transaction = CreateTransaction();

        _repository
            .Setup(repo => repo.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _service.GetByIdAsync(transaction.Id);

        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result!.Id);
        Assert.Equal(transaction.Description, result.Description);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTransactionsMappedToDtos()
    {
        var transactions = new List<PurchaseTransaction> { CreateTransaction(), CreateTransaction() };

        _repository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_WhenTransactionDoesNotExist_ReturnsNullWithoutCallingUpdate()
    {
        var id = Guid.NewGuid();
        var request = new UpdatePurchaseTransactionRequest("Updated", new DateOnly(2026, 2, 1), 10.00m);

        _repository
            .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseTransaction?)null);

        var result = await _service.UpdateAsync(id, request);

        Assert.Null(result);
        _repository.Verify(repo => repo.UpdateAsync(It.IsAny<PurchaseTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenTransactionExists_UpdatesFieldsAndReturnsMappedDto()
    {
        var transaction = CreateTransaction();
        var request = new UpdatePurchaseTransactionRequest("Updated description", new DateOnly(2026, 2, 1), 99.99m);

        _repository
            .Setup(repo => repo.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _service.UpdateAsync(transaction.Id, request);

        Assert.NotNull(result);
        Assert.Equal("Updated description", result!.Description);
        Assert.Equal(99.99m, result.PurchaseAmountUsd);
        _repository.Verify(repo => repo.UpdateAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepositoryAndReturnsItsResult()
    {
        var id = Guid.NewGuid();

        _repository
            .Setup(repo => repo.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(id);

        Assert.True(result);
    }

    private static PurchaseTransaction CreateTransaction()
    {
        return new PurchaseTransaction
        {
            Id = Guid.CreateVersion7(),
            Description = "Office supplies",
            TransactionDate = new DateOnly(2026, 1, 15),
            PurchaseAmountUsd = 42.50m,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
