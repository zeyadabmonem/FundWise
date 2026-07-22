using FundWise.Domain.Common;
using FundWise.Domain.Enums;

namespace FundWise.Domain.Entities;

/// <summary>
/// Represents a financial transaction captured through any channel.
/// </summary>
public sealed class Transaction : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Merchant { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public TransactionCategory Category { get; private set; }
    public CaptureSource Source { get; private set; }
    public DateTime CaptureDate { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Whether the user has confirmed this auto-captured transaction.
    /// All auto-captured transactions start as unconfirmed.
    /// </summary>
    public bool IsConfirmed { get; private set; }

    /// <summary>
    /// AI categorization confidence score (0.0 – 1.0).
    /// Below 0.5 → flagged as "Uncategorized – review".
    /// </summary>
    public double ConfidenceScore { get; private set; }

    // Navigation
    public User User { get; private set; } = default!;

    private Transaction() { } // EF Core constructor

    public static Transaction Create(
        Guid userId,
        string merchant,
        decimal amount,
        string currency,
        TransactionCategory category,
        CaptureSource source,
        DateTime captureDate,
        double confidenceScore = 1.0,
        string? notes = null,
        bool isConfirmed = false)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount must be positive.");

        return new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Merchant = string.IsNullOrWhiteSpace(merchant) ? "Unknown Merchant" : merchant.Trim(),
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Category = category,
            Source = source,
            CaptureDate = captureDate,
            ConfidenceScore = Math.Clamp(confidenceScore, 0.0, 1.0),
            Notes = notes,
            IsConfirmed = isConfirmed,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Confirm()
    {
        IsConfirmed = true;
        SetUpdated();
    }

    public void UpdateCategory(TransactionCategory newCategory)
    {
        Category = newCategory;
        ConfidenceScore = 1.0; // User override = full confidence
        SetUpdated();
    }

    public void UpdateDetails(string? merchant, decimal? amount, TransactionCategory? category, DateTime? captureDate, string? notes)
    {
        if (!string.IsNullOrWhiteSpace(merchant))
            Merchant = merchant.Trim();
        if (amount.HasValue && amount > 0)
            Amount = amount.Value;
        if (category.HasValue)
            Category = category.Value;
        if (captureDate.HasValue)
            CaptureDate = captureDate.Value;
        if (notes is not null)
            Notes = notes;

        SetUpdated();
    }

    /// <summary>Returns true if this transaction is below the confidence threshold and needs user review.</summary>
    public bool NeedsReview => !IsConfirmed && ConfidenceScore < 0.5;
}
