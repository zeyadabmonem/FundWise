using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Application.Features.Transactions.Commands;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Domain.Enums;

namespace FundWise.Application.Features.Voice.Commands;

/// <summary>
/// F2 — Process a voice audio stream into a captured transaction ready for user confirmation.
/// </summary>
public sealed record ProcessVoiceCaptureCommand(
    Guid UserId,
    Stream AudioStream,
    string MimeType) : IRequest<Result<CaptureConfirmationDto>>;

public sealed class ProcessVoiceCaptureHandler : IRequestHandler<ProcessVoiceCaptureCommand, Result<CaptureConfirmationDto>>
{
    private readonly IVoiceService _voiceService;
    private readonly ISender _mediator;

    public ProcessVoiceCaptureHandler(IVoiceService voiceService, ISender mediator)
    { _voiceService = voiceService; _mediator = mediator; }

    public async Task<Result<CaptureConfirmationDto>> Handle(ProcessVoiceCaptureCommand request, CancellationToken cancellationToken)
    {
        var captured = await _voiceService.ProcessAudioAsync(request.AudioStream, request.MimeType, cancellationToken);

        if (captured is null)
            return Error.External("Voice", "Could not extract transaction details from audio. Please try again or enter manually.");

        // Create the transaction as unconfirmed — user must confirm on the confirmation card
        var createResult = await _mediator.Send(new CreateTransactionCommand(
            UserId: request.UserId,
            Merchant: captured.Merchant,
            Amount: captured.Amount,
            Currency: captured.Currency,
            Category: null, // Let the categorization pipeline handle it
            Source: CaptureSource.Voice,
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
            "Voice",
            captured.RawInput);
    }
}
