using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FundWise.Domain.Entities;

namespace FundWise.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("EGP");

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(256);

        builder.HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.MerchantMemories)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Users");
    }
}

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Merchant)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("EGP");

        builder.Property(t => t.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Source)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.Property(t => t.ConfidenceScore)
            .HasDefaultValue(1.0);

        builder.HasIndex(t => new { t.UserId, t.CaptureDate })
            .HasDatabaseName("IX_Transactions_UserId_CaptureDate");

        builder.HasIndex(t => new { t.UserId, t.Category })
            .HasDatabaseName("IX_Transactions_UserId_Category");

        builder.ToTable("Transactions");
    }
}

public sealed class MerchantCategoryMemoryConfiguration : IEntityTypeConfiguration<MerchantCategoryMemory>
{
    public void Configure(EntityTypeBuilder<MerchantCategoryMemory> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MerchantKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.MerchantDisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Unique per user per merchant
        builder.HasIndex(m => new { m.UserId, m.MerchantKey })
            .IsUnique()
            .HasDatabaseName("IX_MerchantMemory_UserId_MerchantKey");

        builder.ToTable("MerchantCategoryMemories");
    }
}

public sealed class AlternativeProductConfiguration : IEntityTypeConfiguration<AlternativeProduct>
{
    public void Configure(EntityTypeBuilder<AlternativeProduct> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.ProductBrand).IsRequired().HasMaxLength(100);
        builder.Property(a => a.AlternativeName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.AlternativeBrand).IsRequired().HasMaxLength(100);

        builder.Property(a => a.CurrentPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.AlternativePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Notes).HasMaxLength(500);

        // SavingAmount and SavingPercentage are computed properties — ignore in DB
        builder.Ignore(a => a.SavingAmount);
        builder.Ignore(a => a.SavingPercentage);

        builder.ToTable("AlternativeProducts");
    }
}
