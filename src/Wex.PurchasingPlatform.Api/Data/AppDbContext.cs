using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Entities;

namespace Wex.PurchasingPlatform.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private const int DescriptionMaxLength = 50;
    private const int PurchaseAmountPrecision = 18;
    private const int PurchaseAmountScale = 2;

    public DbSet<PurchaseTransaction> PurchaseTransactions => Set<PurchaseTransaction>();

    public DbSet<CurrencyOption> CurrencyOptions => Set<CurrencyOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseTransaction>(entity =>
        {
            // TransactionNumber (not Id) is the database primary key so SQLite can generate it natively
            // via its rowid/AUTOINCREMENT mechanism, giving users a short, sequential number to reference
            // instead of a GUID. Id remains the API-facing unique identifier everywhere else in the app.
            entity.HasKey(transaction => transaction.TransactionNumber);
            entity.Property(transaction => transaction.TransactionNumber).ValueGeneratedOnAdd();

            entity.HasIndex(transaction => transaction.Id).IsUnique();

            entity.Property(transaction => transaction.Description)
                .IsRequired()
                .HasMaxLength(DescriptionMaxLength);

            entity.Property(transaction => transaction.PurchaseAmountUsd)
                .HasPrecision(PurchaseAmountPrecision, PurchaseAmountScale);
        });

        modelBuilder.Entity<CurrencyOption>(entity =>
        {
            entity.HasKey(option => option.Id);

            entity.Property(option => option.Country)
                .IsRequired();

            entity.Property(option => option.CurrencyName)
                .IsRequired();

            entity.HasIndex(option => new { option.Country, option.CurrencyName })
                .IsUnique();
        });
    }
}
