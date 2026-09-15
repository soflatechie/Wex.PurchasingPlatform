using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;

namespace Wex.PurchasingPlatform.Tests.Repositories;

public class ExchangeRateRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly AppDbContext _context;
    private readonly ExchangeRateRepository _repository;

    public ExchangeRateRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"wex-purchasing-tests-{Guid.NewGuid()}.db");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.Migrate();

        _repository = new ExchangeRateRepository(_context);
    }

    [Fact]
    public async Task AddAsync_WhenCalled_PersistsQuote()
    {
        var quote = CreateQuote("United Kingdom", "Pound", new DateOnly(2026, 3, 31), 0.79m);

        await _repository.AddAsync(quote);
        var storedQuotes = await _repository.GetAllAsync();

        Assert.Single(storedQuotes);
        Assert.Equal("Pound", storedQuotes[0].CurrencyName);
    }

    [Fact]
    public async Task AddAsync_WhenCountryCurrencyRecordDateAlreadyExists_ThrowsDueToUniqueConstraint()
    {
        await _repository.AddAsync(CreateQuote("United Kingdom", "Pound", new DateOnly(2026, 3, 31), 0.79m));

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _repository.AddAsync(CreateQuote("United Kingdom", "Pound", new DateOnly(2026, 3, 31), 0.80m)));
    }

    [Fact]
    public async Task DeleteAsync_WhenQuoteExists_RemovesItAndReturnsTrue()
    {
        var quote = CreateQuote("Canada", "Dollar", new DateOnly(2026, 3, 31), 1.35m);
        await _repository.AddAsync(quote);

        var wasDeleted = await _repository.DeleteAsync(quote.Id);
        var storedQuote = await _repository.GetByIdAsync(quote.Id);

        Assert.True(wasDeleted);
        Assert.Null(storedQuote);
    }

    public void Dispose()
    {
        _context.Dispose();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    private static ExchangeRateQuote CreateQuote(string country, string currencyName, DateOnly recordDate, decimal exchangeRate)
    {
        return new ExchangeRateQuote
        {
            Country = country,
            CurrencyName = currencyName,
            RecordDate = recordDate,
            ExchangeRate = exchangeRate,
            FetchedAtUtc = DateTime.UtcNow
        };
    }
}
