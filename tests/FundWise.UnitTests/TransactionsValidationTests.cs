using Xunit;
using FundWise.Application.Features.Transactions.Commands;
using FundWise.Domain.Enums;

namespace FundWise.UnitTests;

public class TransactionsValidationTests
{
    private readonly CreateTransactionValidator _validator = new();

    [Fact]
    public void Validate_AmountZeroOrNegative_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            UserId: Guid.NewGuid(),
            Merchant: "Starbucks",
            Amount: 0m, // Invalid: amount must be > 0 per Business Rules
            Currency: "EGP",
            Category: TransactionCategory.FoodAndDrink,
            Source: CaptureSource.Manual,
            CaptureDate: DateTime.UtcNow,
            Notes: null,
            IsConfirmed: true);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_ValidTransaction_ShouldPassValidation()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            UserId: Guid.NewGuid(),
            Merchant: "Starbucks",
            Amount: 120.50m,
            Currency: "EGP",
            Category: TransactionCategory.FoodAndDrink,
            Source: CaptureSource.Manual,
            CaptureDate: DateTime.UtcNow,
            Notes: "Morning coffee",
            IsConfirmed: true);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
