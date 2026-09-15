using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;

namespace Wex.PurchasingPlatform.Tests.Repositories;

public class PurchaseTransactionRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly AppDbContext _context;
    private readonly PurchaseTransactionRepository _repository;

    public PurchaseTransactionRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"wex-purchasing-tests-{Guid.NewGuid()}.db");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.Migrate();

        _repository = new PurchaseTransactionRepository(_context);
    }

    [Fact]
    public async Task AddAsync_WhenCalled_PersistsTransaction()
    {
        var transaction = CreateTransaction("Office supplies");

        await _repository.AddAsync(transaction);
        var storedTransaction = await _repository.GetByIdAsync(transaction.Id);

        Assert.NotNull(storedTransaction);
        Assert.Equal(transaction.Description, storedTransaction!.Description);
        Assert.Equal(transaction.PurchaseAmountUsd, storedTransaction.PurchaseAmountUsd);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionDoesNotExist_ReturnsNull()
    {
        var storedTransaction = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(storedTransaction);
    }

    [Fact]
    public async Task GetAllAsync_WhenMultipleTransactionsExist_ReturnsAll()
    {
        await _repository.AddAsync(CreateTransaction("First purchase"));
        await _repository.AddAsync(CreateTransaction("Second purchase"));

        var transactions = await _repository.GetAllAsync();

        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_PersistsChanges()
    {
        var transaction = CreateTransaction("Original description");
        await _repository.AddAsync(transaction);

        transaction.Description = "Updated description";
        await _repository.UpdateAsync(transaction);

        var storedTransaction = await _repository.GetByIdAsync(transaction.Id);

        Assert.Equal("Updated description", storedTransaction!.Description);
    }

    [Fact]
    public async Task DeleteAsync_WhenTransactionExists_RemovesItAndReturnsTrue()
    {
        var transaction = CreateTransaction("To be deleted");
        await _repository.AddAsync(transaction);

        var wasDeleted = await _repository.DeleteAsync(transaction.Id);
        var storedTransaction = await _repository.GetByIdAsync(transaction.Id);

        Assert.True(wasDeleted);
        Assert.Null(storedTransaction);
    }

    [Fact]
    public async Task DeleteAsync_WhenTransactionDoesNotExist_ReturnsFalse()
    {
        var wasDeleted = await _repository.DeleteAsync(Guid.NewGuid());

        Assert.False(wasDeleted);
    }

    public void Dispose()
    {
        _context.Dispose();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    private static PurchaseTransaction CreateTransaction(string description)
    {
        return new PurchaseTransaction
        {
            Id = Guid.NewGuid(),
            Description = description,
            TransactionDate = new DateOnly(2026, 1, 15),
            PurchaseAmountUsd = 10.00m,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
