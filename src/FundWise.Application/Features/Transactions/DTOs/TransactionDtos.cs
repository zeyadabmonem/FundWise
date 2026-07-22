using FundWise.Domain.Enums;

namespace FundWise.Application.Features.Transactions.DTOs;

public sealed record TransactionDto(
    Guid Id,
    Guid UserId,
    string Merchant,
    decimal Amount,
    string Currency,
    TransactionCategory Category,
    string CategoryName,
    CaptureSource Source,
    string SourceName,
    DateTime CaptureDate,
    string? Notes,
    bool IsConfirmed,
    bool NeedsReview,
    double ConfidenceScore,
    DateTime CreatedAt);

public sealed record TransactionSummaryDto(
    Guid Id,
    string Merchant,
    decimal Amount,
    string Currency,
    TransactionCategory Category,
    string CategoryName,
    CaptureSource Source,
    DateTime CaptureDate,
    bool IsConfirmed,
    bool NeedsReview);

public sealed record CaptureConfirmationDto(
    string Merchant,
    decimal Amount,
    string Currency,
    TransactionCategory Category,
    string CategoryHint,
    DateTime CaptureDate,
    double ConfidenceScore,
    string CaptureChannel,
    string? RawInput);
