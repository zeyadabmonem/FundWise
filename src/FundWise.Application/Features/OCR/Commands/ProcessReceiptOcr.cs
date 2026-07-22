using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Application.Features.Transactions.Commands;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Domain.Enums;

namespace FundWise.Application.Features.OCR.Commands;

/// <summary>
/// F3 — Process a receipt image via GPT-4o Vision into a captured transaction.
/// </summary>
public sealed record ProcessReceiptOcrCommand(
    Guid UserId,
    Stream ImageStream,
    string MimeType) : IRequest<Result<CaptureConfirmationDto>>;

public sealed class ProcessReceiptOcrHandler : IRequestHandler<ProcessReceiptOcrCommand, Result<CaptureConfirmationDto>>
{
    private readonly IOcrService _ocrService;
    private readonly ISender _mediator;

    public ProcessReceiptOcrHandler(IOcrService ocrService, ISender mediator)
    { _ocrService = ocrService; _mediator = mediator; }

    public async Task<Result<CaptureConfirmationDto>> Handle(ProcessReceiptOcrCommand request, CancellationToken cancellationToken)
    {
        var captured = await _ocrService.ProcessImageAsync(request.ImageStream, request.MimeType, cancellationToken);

        if (captured is null)
            return Error.External("OCR", "Could not extract receipt data from image. Please retake or enter manually.");

        var createResult = await _mediator.Send(new CreateTransactionCommand(
            UserId: request.UserId,
            Merchant: captured.Merchant,
            Amount: captured.Amount,
            Currency: captured.Currency,
            Category: null,
            Source: CaptureSource.ReceiptOCR,
            CaptureDate: captured.Date,
            Notes: null,
            IsConfirmed: false,
            ConfidenceScore: captured.ConfidenceScore),
        cancellationToken);

        if (createResult.IsFailure) return createResult.Error;

        return new CaptureConfirmationDto(
            captured.Merchant,
            captured.Amount,
            captured.Currency,
            createResult.Value.Category,
            captured.CategoryHint ?? createResult.Value.CategoryName,
            captured.Date ?? DateTime.UtcNow,
            captured.ConfidenceScore,
            "Receipt OCR",
            captured.RawInput);
    }
}
