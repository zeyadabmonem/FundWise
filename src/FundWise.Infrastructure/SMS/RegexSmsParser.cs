using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FundWise.Application.Common.Interfaces;

namespace FundWise.Infrastructure.SMS;

/// <summary>
/// F5 — Regex SMS parser tuned specifically for major Egyptian Banks & Wallets.
/// Supports CIB, NBE, Banque Misr, Vodafone Cash, InstaPay SMS formats.
/// </summary>
public sealed class RegexSmsParser : ISmsParser
{
    private readonly ILogger<RegexSmsParser> _logger;

    public RegexSmsParser(ILogger<RegexSmsParser> logger) => _logger = logger;

    public Task<CapturedTransaction?> ParseAsync(string smsBody, string sender, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(smsBody)) return Task.FromResult<CapturedTransaction?>(null);

        var senderUpper = sender.ToUpperInvariant();
        var body = smsBody.Trim();

        // 1. CIB Egypt SMS Format (e.g. "Card ending 1234 used for EGP 250.00 at Starbucks on 22/07/2026")
        var cibMatch = Regex.Match(body, @"used for (?:EGP|LE|جنيه)?\s*([\d,.]+)\s*at\s+([^.\n]+)", RegexOptions.IgnoreCase);
        if (cibMatch.Success && decimal.TryParse(cibMatch.Groups[1].Value.Replace(",", ""), out var cibAmount))
        {
            return Task.FromResult<CapturedTransaction?>(new CapturedTransaction(
                Merchant: cibMatch.Groups[2].Value.Trim(),
                Amount: cibAmount,
                Currency: "EGP",
                CategoryHint: null,
                Date: DateTime.UtcNow,
                ConfidenceScore: 0.95,
                RawInput: body));
        }

        // 2. NBE / Banque Misr (e.g. "تمخصم مبلغ 350.00 جم لدى Carrefour بتاريخ...")
        var nbeMatch = Regex.Match(body, @"خصم\s+(?:مبلغ\s+)?([\d,.]+)\s*(?:جم|EGP)?\s+لدى\s+([^.\n]+)", RegexOptions.IgnoreCase);
        if (nbeMatch.Success && decimal.TryParse(nbeMatch.Groups[1].Value.Replace(",", ""), out var nbeAmount))
        {
            return Task.FromResult<CapturedTransaction?>(new CapturedTransaction(
                Merchant: nbeMatch.Groups[2].Value.Trim(),
                Amount: nbeAmount,
                Currency: "EGP",
                CategoryHint: null,
                Date: DateTime.UtcNow,
                ConfidenceScore: 0.95,
                RawInput: body));
        }

        // 3. Vodafone Cash (e.g. "تم تحويل 100 جنيه إلى 01012345678" or "تم خصم 150 جنيه مقابل...")
        var vfMatch = Regex.Match(body, @"(?:خصم|تحويل)\s+([\d,.]+)\s*جنيه\s+(?:إلى|مقابل|لدى)\s+([^.\n]+)", RegexOptions.IgnoreCase);
        if (vfMatch.Success && decimal.TryParse(vfMatch.Groups[1].Value.Replace(",", ""), out var vfAmount))
        {
            return Task.FromResult<CapturedTransaction?>(new CapturedTransaction(
                Merchant: vfMatch.Groups[2].Value.Trim(),
                Amount: vfAmount,
                Currency: "EGP",
                CategoryHint: null,
                Date: DateTime.UtcNow,
                ConfidenceScore: 0.92,
                RawInput: body));
        }

        // 4. InstaPay (e.g. "Successful transfer of 500.00 EGP to Ahmad Hassan")
        var instaMatch = Regex.Match(body, @"transfer of\s+([\d,.]+)\s*EGP\s+to\s+([^.\n]+)", RegexOptions.IgnoreCase);
        if (instaMatch.Success && decimal.TryParse(instaMatch.Groups[1].Value.Replace(",", ""), out var instaAmount))
        {
            return Task.FromResult<CapturedTransaction?>(new CapturedTransaction(
                Merchant: instaMatch.Groups[2].Value.Trim(),
                Amount: instaAmount,
                Currency: "EGP",
                CategoryHint: "Transfer",
                Date: DateTime.UtcNow,
                ConfidenceScore: 0.95,
                RawInput: body));
        }

        return Task.FromResult<CapturedTransaction?>(null);
    }
}
