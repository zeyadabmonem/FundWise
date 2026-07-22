using FundWise.Domain.Enums;

namespace FundWise.Application.Common.Interfaces;

// ──────────────────────────────────────────────────────────────────────────────
// AI / Capture channel service interfaces
// Infrastructure layer provides the concrete implementations.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Result of structured extraction from any capture channel.
/// </summary>
public sealed record CapturedTransaction(
    string Merchant,
    decimal Amount,
    string Currency,
    string? CategoryHint,
    DateTime? Date,
    double ConfidenceScore,
    string? RawInput = null);

/// <summary>
/// F2 — AI Voice Capture: transcribes audio and extracts transaction fields.
/// </summary>
public interface IVoiceService
{
    /// <summary>
    /// Transcribes the audio stream and extracts structured transaction data.
    /// </summary>
    Task<CapturedTransaction?> ProcessAudioAsync(Stream audioStream, string mimeType, CancellationToken cancellationToken = default);
}

/// <summary>
/// F3 — Receipt OCR: extracts transaction data from a receipt image using GPT-4o Vision.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Analyzes the image and returns extracted receipt data.
    /// </summary>
    Task<CapturedTransaction?> ProcessImageAsync(Stream imageStream, string mimeType, CancellationToken cancellationToken = default);
}

/// <summary>
/// F5 — SMS Parser: parses bank SMS notifications into structured transactions.
/// </summary>
public interface ISmsParser
{
    /// <summary>
    /// Attempts to parse a bank SMS. Returns null if the SMS is not a financial transaction.
    /// </summary>
    Task<CapturedTransaction?> ParseAsync(string smsBody, string sender, CancellationToken cancellationToken = default);
}

/// <summary>
/// F4 — QR Code Parser: decodes and interprets QR receipt content.
/// </summary>
public interface IQrParser
{
    /// <summary>
    /// Decodes QR content and extracts transaction data if it's a recognized receipt format.
    /// Returns null if the QR is not a financial receipt.
    /// </summary>
    Task<CapturedTransaction?> ParseAsync(string qrContent, CancellationToken cancellationToken = default);
}

/// <summary>
/// F7 — AI Expense Categorization: assigns a category + confidence to a transaction.
/// </summary>
public interface ICategorizationService
{
    Task<(TransactionCategory Category, double Confidence)> CategorizeAsync(
        string merchant,
        decimal amount,
        string? description = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// F9 — AI Alternatives Engine (Phase 1: mock data only).
/// </summary>
public interface IAlternativesService
{
    Task<AlternativeRecommendation?> GetAlternativeAsync(
        string merchant,
        string? productName,
        decimal amount,
        TransactionCategory category,
        CancellationToken cancellationToken = default);
}

public sealed record AlternativeRecommendation(
    string CurrentProductName,
    decimal CurrentPrice,
    string AlternativeName,
    string AlternativeBrand,
    decimal AlternativePrice,
    decimal SavingAmount,
    double SavingPercentage,
    decimal ProjectedMonthlySaving,
    decimal ProjectedYearlySaving);

/// <summary>
/// JWT token service for generating and validating tokens.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string name);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}

/// <summary>
/// Password hashing service (BCrypt implementation in Infrastructure).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>
/// File storage for uploaded receipt images.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
