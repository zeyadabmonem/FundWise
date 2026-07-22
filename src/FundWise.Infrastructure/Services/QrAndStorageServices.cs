using System.Text.Json;
using System.Text.RegularExpressions;
using FundWise.Application.Common.Interfaces;

namespace FundWise.Infrastructure.Services;

/// <summary>
/// F4 — QR Code content parser (supports simple structured JSON payloads and receipt URLs).
/// </summary>
public sealed class QrContentParser : IQrParser
{
    public Task<CapturedTransaction?> ParseAsync(string qrContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(qrContent))
            return Task.FromResult<CapturedTransaction?>(null);

        var content = qrContent.Trim();

        // 1. Check if payload is structured JSON
        if (content.StartsWith("{") && content.EndsWith("}"))
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var merchant = root.TryGetProperty("merchant", out var m) ? m.GetString() :
                               root.TryGetProperty("vendor", out var v) ? v.GetString() : "QR Merchant";
                var amount = root.TryGetProperty("amount", out var a) ? a.GetDecimal() :
                             root.TryGetProperty("total", out var t) ? t.GetDecimal() : 0m;
                var currency = root.TryGetProperty("currency", out var c) ? c.GetString() ?? "EGP" : "EGP";

                if (amount > 0)
                {
                    return Task.FromResult<CapturedTransaction?>(new CapturedTransaction(
                        Merchant: merchant ?? "QR Merchant",
                        Amount: amount,
                        Currency: currency,
                        CategoryHint: null,
                        Date: DateTime.UtcNow,
                        ConfidenceScore: 0.98,
                        RawInput: content));
                }
            }
            catch
            {
                // Fall through to regex URL parsing if JSON parsing fails
            }
        }

        // 2. Check if payload is URL with amount parameters (common in e-wallets / payment links)
        var amountMatch = Regex.Match(content, @"(?:amount|total|sum)=([\d,.]+)", RegexOptions.IgnoreCase);
        var merchantMatch = Regex.Match(content, @"(?:merchant|vendor|seller)=([^&]+)", RegexOptions.IgnoreCase);

        if (amountMatch.Success && decimal.TryParse(amountMatch.Groups[1].Value, out var urlAmount) && urlAmount > 0)
        {
            var merchantName = merchantMatch.Success ? Uri.UnescapeDataString(merchantMatch.Groups[1].Value) : "QR Payment";
            return Task.FromResult<CapturedTransaction?>(new CapturedTransaction(
                Merchant: merchantName,
                Amount: urlAmount,
                Currency: "EGP",
                CategoryHint: null,
                Date: DateTime.UtcNow,
                ConfidenceScore: 0.90,
                RawInput: content));
        }

        return Task.FromResult<CapturedTransaction?>(null);
    }
}

/// <summary>
/// File storage service for uploaded receipt images. Stores files locally under wwwroot/uploads.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;

    public LocalFileStorageService()
    {
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_uploadPath, uniqueName);

        using var outputStream = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        return $"/uploads/{uniqueName}";
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);
        var fullPath = Path.Combine(_uploadPath, fileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
