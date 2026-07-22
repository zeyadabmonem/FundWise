using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Application.Features.Transactions.Commands;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Domain.Enums;

namespace FundWise.Application.Features.QR.Commands;

/// <summary>
/// F4 — Parse a QR code from a digital receipt into a captured transaction.
/// </summary>
public sealed record ParseQrCodeCommand(
    Guid UserId,
    string QrContent) : IRequest<Result<CaptureConfirmationDto>>;

public sealed class ParseQrCodeHandler : IRequestHandler<ParseQrCodeCommand, Result<CaptureConfirmationDto>>
{
    private readonly IQrParser _qrParser;
    private readonly ISender _mediator;

    public ParseQrCodeHandler(IQrParser qrParser, ISender mediator)
    { _qrParser = qrParser; _mediator = mediator; }

    public async Task<Result<CaptureConfirmationDto>> Handle(ParseQrCodeCommand request, CancellationToken cancellationToken)
    {
        var captured = await _qrParser.ParseAsync(request.QrContent, cancellationToken);

        if (captured is null)
            return Error.External("QR", "This QR code is not a recognized receipt format. Please enter manually.");

        var createResult = await _mediator.Send(new CreateTransactionCommand(
            UserId: request.UserId,
            Merchant: captured.Merchant,
            Amount: captured.Amount,
            Currency: captured.Currency,
            Category: null,
            Source: CaptureSource.QRCode,
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
            "QR Code",
            request.QrContent);
    }
}
