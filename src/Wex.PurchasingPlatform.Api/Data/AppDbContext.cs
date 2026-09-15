using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Entities;

namespace Wex.PurchasingPlatform.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private const int DescriptionMaxLength = 50;
    private const int PurchaseAmountPrecision = 18;
    private const int PurchaseAmountScale = 2;
    private const int ExchangeRatePrecision = 18;
    private const int ExchangeRateScale = 6;

    public DbSet<PurchaseTransaction> PurchaseTransactions => Set<PurchaseTransaction>();

    public DbSet<ExchangeRateQuote> ExchangeRateQuotes => Set<ExchangeRateQuote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseTransaction>(entity =>
        {
            entity.HasKey(transaction => transaction.Id);

            entity.Property(transaction => transaction.Description)
                .IsRequired()
                .HasMaxLength(DescriptionMaxLength);

            entity.Property(transaction => transaction.PurchaseAmountUsd)
                .HasPrecision(PurchaseAmountPrecision, PurchaseAmountScale);
        });

        modelBuilder.Entity<ExchangeRateQuote>(entity =>
        {
            entity.HasKey(quote => quote.Id);

            entity.Property(quote => quote.Country)
                .IsRequired();

            entity.Property(quote => quote.CurrencyName)
                .IsRequired();

            entity.Property(quote => quote.ExchangeRate)
                .HasPrecision(ExchangeRatePrecision, ExchangeRateScale);

            entity.HasIndex(quote => new { quote.Country, quote.CurrencyName, quote.RecordDate })
                .IsUnique()
                .IsDescending(false, false, true);
        });
    }
}
