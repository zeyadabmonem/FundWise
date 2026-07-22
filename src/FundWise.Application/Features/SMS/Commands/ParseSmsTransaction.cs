using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Application.Features.Transactions.Commands;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Domain.Enums;

namespace FundWise.Application.Features.SMS.Commands;

/// <summary>
/// F5 — Parse a bank SMS notification into a captured transaction.
/// </summary>
public sealed record ParseSmsTransactionCommand(
    Guid UserId,
    string SmsBody,
    string Sender) : IRequest<Result<CaptureConfirmationDto>>;

public sealed class ParseSmsTransactionHandler : IRequestHandler<ParseSmsTransactionCommand, Result<CaptureConfirmationDto>>
{
    private readonly ISmsParser _smsParser;
    private readonly ISender _mediator;

    public ParseSmsTransactionHandler(ISmsParser smsParser, ISender mediator)
    { _smsParser = smsParser; _mediator = mediator; }

    public async Task<Result<CaptureConfirmationDto>> Handle(ParseSmsTransactionCommand request, CancellationToken cancellationToken)
    {
        var captured = await _smsParser.ParseAsync(request.SmsBody, request.Sender, cancellationToken);

        if (captured is null)
            return Error.External("SMS", "This SMS does not appear to be a financial transaction or is from an unrecognized sender.");

        var createResult = await _mediator.Send(new CreateTransactionCommand(
            UserId: request.UserId,
            Merchant: captured.Merchant,
            Amount: captured.Amount,
            Currency: captured.Currency,
            Category: null,
            Source: CaptureSource.SMS,
            CaptureDate: captured.Date,
            Notes: $"SMS from {request.Sender}",
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
            "SMS",
            request.SmsBody);
    }
}
