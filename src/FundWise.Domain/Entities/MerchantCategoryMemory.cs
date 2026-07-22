using FundWise.Domain.Common;
using FundWise.Domain.Enums;

namespace FundWise.Domain.Entities;

/// <summary>
/// Per-user learned merchant → category mapping.
/// When a user corrects a categorization, we store it here so future
/// transactions from the same merchant are correctly categorized without AI.
/// </summary>
public sealed class MerchantCategoryMemory : BaseEntity
{
    public Guid UserId { get; private set; }

    /// <summary>Normalized lowercase merchant name used for matching.</summary>
    public string MerchantKey { get; private set; } = default!;

    /// <summary>Original merchant name as displayed.</summary>
    public string MerchantDisplayName { get; private set; } = default!;

    public TransactionCategory Category { get; private set; }

    // Navigation
    public User User { get; private set; } = default!;

    private MerchantCategoryMemory() { } // EF Core constructor

    public static MerchantCategoryMemory Create(Guid userId, string merchantName, TransactionCategory category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(merchantName);

        return new MerchantCategoryMemory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MerchantKey = merchantName.Trim().ToLowerInvariant(),
            MerchantDisplayName = merchantName.Trim(),
            Category = category,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateCategory(TransactionCategory newCategory)
    {
        Category = newCategory;
        SetUpdated();
    }
}
