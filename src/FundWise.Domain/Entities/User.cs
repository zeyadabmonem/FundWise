using FundWise.Domain.Common;

namespace FundWise.Domain.Entities;

/// <summary>
/// Represents a registered FundWise user.
/// </summary>
public sealed class User : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Currency { get; private set; } = "EGP";
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    // Navigation
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<MerchantCategoryMemory> MerchantMemories { get; private set; } = new List<MerchantCategoryMemory>();

    private User() { } // EF Core constructor

    public static User Create(string name, string email, string passwordHash, string currency = "EGP")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Currency = currency,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string? name, string? currency)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();
        if (!string.IsNullOrWhiteSpace(currency))
            Currency = currency.Trim().ToUpperInvariant();
        SetUpdated();
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
        SetUpdated();
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
        SetUpdated();
    }

    public bool IsRefreshTokenValid(string token)
        => RefreshToken == token && RefreshTokenExpiry.HasValue && RefreshTokenExpiry > DateTime.UtcNow;
}
