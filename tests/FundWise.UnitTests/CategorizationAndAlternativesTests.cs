using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using FundWise.Infrastructure.AI;
using FundWise.Domain.Enums;
using FundWise.Domain.Entities;
using FundWise.Domain.Interfaces;
using Moq;

namespace FundWise.UnitTests;

public class CategorizationAndAlternativesTests
{
    private readonly OpenAiCategorizationService _categorizationService;

    public CategorizationAndAlternativesTests()
    {
        // Mock IConfiguration with empty API key to force rule-based fast path (no OpenAI calls)
        var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfig.Setup(c => c["OpenAI:ApiKey"]).Returns(string.Empty);
        _categorizationService = new OpenAiCategorizationService(mockConfig.Object, NullLogger<OpenAiCategorizationService>.Instance);
    }

    [Theory]
    [InlineData("Starbucks Cafe", TransactionCategory.FoodAndDrink)]
    [InlineData("Carrefour Express", TransactionCategory.Groceries)]
    [InlineData("Uber Ride", TransactionCategory.Transport)]
    [InlineData("Vodafone Egypt", TransactionCategory.BillsAndUtilities)]
    [InlineData("InstaPay Transfer", TransactionCategory.Transfer)]
    public async Task CategorizeAsync_FastCategoryRules_ReturnsExpectedCategory(string merchant, TransactionCategory expected)
    {
        // Act
        var result = await _categorizationService.CategorizeAsync(merchant, 100m);

        // Assert
        Assert.Equal(expected, result.Category);
        Assert.True(result.Confidence >= 0.9);
    }

    [Fact]
    public async Task GetAlternativeAsync_CalculatesMonthlyAndYearlySavingsCorrectly()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<AlternativeProduct>>();
        var sampleAlternatives = new List<AlternativeProduct>
        {
            AlternativeProduct.Create(
                productName: "Nespresso Coffee Capsule",
                productBrand: "Nespresso",
                category: TransactionCategory.FoodAndDrink,
                currentPrice: 35.00m,
                alternativeName: "Local Roast Pods",
                alternativeBrand: "Abu Auf",
                alternativePrice: 15.00m) // SavingAmount and SavingPercentage are computed properties
        };

        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleAlternatives);

        var alternativesService = new OpenAiAlternativesService(mockRepo.Object);

        // Act
        var recommendation = await alternativesService.GetAlternativeAsync(
            merchant: "Nespresso",
            productName: "Nespresso Capsule",
            amount: 35.00m,
            category: TransactionCategory.FoodAndDrink);

        // Assert
        Assert.NotNull(recommendation);
        Assert.Equal(20.00m, recommendation.SavingAmount);       // 35 - 15 = 20
        Assert.Equal(80.00m, recommendation.ProjectedMonthlySaving);  // 20 * 4
        Assert.Equal(960.00m, recommendation.ProjectedYearlySaving); // 80 * 12
    }
}
