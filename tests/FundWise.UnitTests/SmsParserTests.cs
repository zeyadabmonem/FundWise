using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using FundWise.Infrastructure.SMS;

namespace FundWise.UnitTests;

public class SmsParserTests
{
    private readonly RegexSmsParser _parser;

    public SmsParserTests()
    {
        _parser = new RegexSmsParser(NullLogger<RegexSmsParser>.Instance);
    }

    [Fact]
    public async Task ParseAsync_CibSms_ReturnsCorrectTransaction()
    {
        // Arrange
        var smsBody = "Card ending 1234 used for EGP 250.00 at Starbucks. Transaction approved.";
        var sender = "CIB";

        // Act
        var result = await _parser.ParseAsync(smsBody, sender);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Starbucks", result.Merchant);
        Assert.Equal(250.00m, result.Amount);
        Assert.Equal("EGP", result.Currency);
        Assert.True(result.ConfidenceScore >= 0.9);
    }

    [Fact]
    public async Task ParseAsync_NbeSms_ReturnsCorrectTransaction()
    {
        // Arrange
        var smsBody = "تم خصم مبلغ 350.00 جم لدى Carrefour. بتاريخ 22/07/2026";
        var sender = "NBE";

        // Act
        var result = await _parser.ParseAsync(smsBody, sender);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Carrefour", result.Merchant);
        Assert.Equal(350.00m, result.Amount);
        Assert.Equal("EGP", result.Currency);
    }

    [Fact]
    public async Task ParseAsync_VodafoneCashSms_ReturnsCorrectTransaction()
    {
        // Arrange
        var smsBody = "تم خصم 150 جنيه مقابل Uber";
        var sender = "VF-Cash";

        // Act
        var result = await _parser.ParseAsync(smsBody, sender);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Uber", result.Merchant);
        Assert.Equal(150.00m, result.Amount);
    }

    [Fact]
    public async Task ParseAsync_InstaPaySms_ReturnsCorrectTransaction()
    {
        // Arrange
        var smsBody = "Successful transfer of 500.00 EGP to Ahmad Hassan";
        var sender = "InstaPay";

        // Act
        var result = await _parser.ParseAsync(smsBody, sender);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ahmad Hassan", result.Merchant);
        Assert.Equal(500.00m, result.Amount);
        Assert.Equal("Transfer", result.CategoryHint);
    }

    [Fact]
    public async Task ParseAsync_InvalidSmsFormat_ReturnsNullGracefully()
    {
        // Arrange
        var smsBody = "Welcome to Egypt! Your roaming package is activated.";
        var sender = "Vodafone";

        // Act
        var result = await _parser.ParseAsync(smsBody, sender);

        // Assert
        Assert.Null(result); // Graceful degradation, no crash
    }
}
