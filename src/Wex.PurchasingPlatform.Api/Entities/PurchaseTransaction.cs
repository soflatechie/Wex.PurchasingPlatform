namespace Wex.PurchasingPlatform.Api.Entities;

public class PurchaseTransaction
{
    public int TransactionNumber { get; set; }

    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateOnly TransactionDate { get; set; }

    public decimal PurchaseAmountUsd { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
