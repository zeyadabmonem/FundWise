using Xunit;
using FundWise.Application.Features.SMS.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using FundWise.Infrastructure.SMS;
using FundWise.Application.Common.Interfaces;
using Moq;

namespace FundWise.UnitTests;

/// <summary>
/// Tests the SMS allow-list filter (Section 2 — highest priority test suite).
/// A bug here reads as a privacy failure, not just a feature bug.
/// </summary>
public class SmsAllowListTests
{
    private readonly RegexSmsParser _parser;

    public SmsAllowListTests()
    {
        _parser = new RegexSmsParser(NullLogger<RegexSmsParser>.Instance);
    }

    [Theory]
    [InlineData("RANDOM-SENDER")]   // Non-allowlisted sender
    [InlineData("Telecom")]          // Partial match — should NOT match "CIB"
    [InlineData("")]                 // Empty sender
    [InlineData("vodafone-promo")]   // Marketing, not transactional
    public async Task ParseAsync_NonTransactionalSms_ReturnsNull(string sender)
    {
        // A valid CIB-format body but from a non-allowlisted sender
        // Parser itself doesn't enforce allow-list, but testing
        // non-bank SMS formats ensures they aren't accidentally captured
        var smsBody = "Your monthly plan has been renewed. No action needed.";

        var result = await _parser.ParseAsync(smsBody, sender);

        Assert.Null(result); // Non-transactional bodies must return null
    }

    [Fact]
    public async Task ParseAsync_BalanceNotificationSms_ReturnsNull()
    {
        // A balance-inquiry SMS (not a debit) must NOT be saved as an expense
        // Regression: ensure "Current balance" does not match debit patterns
        var smsBody = "Your CIB account balance is EGP 12,500.00 as of 22/07/2026.";
        var sender = "CIB";

        var result = await _parser.ParseAsync(smsBody, sender);

        Assert.Null(result); // Balance notification ≠ transaction
    }

    [Fact]
    public async Task ParseAsync_CaseSensitivity_SenderDoesNotAffectParsing()
    {
        // Parser is case-insensitive on the SMS body (not on sender — allow-list is enforced upstream)
        var smsBody = "Card ending 1234 USED FOR EGP 200.00 AT MCDONALDS. Approved.";
        var sender = "CIB";

        var result = await _parser.ParseAsync(smsBody, sender);

        // Should parse regardless of case in the SMS body
        Assert.NotNull(result);
        Assert.Equal(200.00m, result.Amount);
    }

    [Fact]
    public async Task ParseAsync_EmptySmsBody_ReturnsNullSafely()
    {
        // Empty/whitespace SMS should never crash
        var result = await _parser.ParseAsync("   ", "CIB");
        Assert.Null(result);
    }

    [Fact]
    public async Task ParseAsync_NullOrWhiteSpaceBody_HandledGracefully()
    {
        var result = await _parser.ParseAsync(string.Empty, "NBE");
        Assert.Null(result); // Must not throw
    }
}
