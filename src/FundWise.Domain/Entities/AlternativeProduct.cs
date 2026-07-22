using FundWise.Domain.Common;
using FundWise.Domain.Enums;

namespace FundWise.Domain.Entities;

/// <summary>
/// Mock product dataset for the F9 AI Alternatives Engine (Phase 1 demo scope).
/// Seeded manually — no live pricing data.
/// </summary>
public sealed class AlternativeProduct : BaseEntity
{
    public string ProductName { get; private set; } = default!;
    public string ProductBrand { get; private set; } = default!;
    public TransactionCategory Category { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public string AlternativeName { get; private set; } = default!;
    public string AlternativeBrand { get; private set; } = default!;
    public decimal AlternativePrice { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>Calculated saving per unit.</summary>
    public decimal SavingAmount => CurrentPrice - AlternativePrice;

    /// <summary>Saving as a percentage of current price.</summary>
    public double SavingPercentage => CurrentPrice > 0
        ? Math.Round((double)SavingAmount / (double)CurrentPrice * 100, 1)
        : 0;

    private AlternativeProduct() { } // EF Core constructor

    public static AlternativeProduct Create(
        string productName,
        string productBrand,
        TransactionCategory category,
        decimal currentPrice,
        string alternativeName,
        string alternativeBrand,
        decimal alternativePrice,
        string? notes = null)
    {
        if (alternativePrice >= currentPrice)
            throw new ArgumentException("Alternative price must be lower than current price.");

        return new AlternativeProduct
        {
            Id = Guid.NewGuid(),
            ProductName = productName.Trim(),
            ProductBrand = productBrand.Trim(),
            Category = category,
            CurrentPrice = currentPrice,
            AlternativeName = alternativeName.Trim(),
            AlternativeBrand = alternativeBrand.Trim(),
            AlternativePrice = alternativePrice,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }
}
