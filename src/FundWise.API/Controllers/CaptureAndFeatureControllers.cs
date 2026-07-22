using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FundWise.Application.Features.Dashboard.Queries;
using FundWise.Application.Features.OCR.Commands;
using FundWise.Application.Features.QR.Commands;
using FundWise.Application.Features.Recommendations.Queries;
using FundWise.Application.Features.SMS.Commands;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Application.Features.Voice.Commands;

namespace FundWise.API.Controllers;

[Authorize]
public sealed class VoiceController : BaseApiController
{
    /// <summary>
    /// F2 — Voice Capture: upload audio file to extract transaction.
    /// </summary>
    [HttpPost("capture")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CaptureConfirmationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CaptureVoice(IFormFile audioFile)
    {
        if (audioFile is null || audioFile.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Audio file is required." });

        using var stream = audioFile.OpenReadStream();
        var result = await Mediator.Send(new ProcessVoiceCaptureCommand(UserId, stream, audioFile.ContentType));
        return HandleResult(result);
    }
}

[Authorize]
public sealed class OcrController : BaseApiController
{
    /// <summary>
    /// F3 — Receipt OCR Scanner: upload photo of receipt to extract transaction via GPT-4o Vision.
    /// </summary>
    [HttpPost("receipt")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CaptureConfirmationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ScanReceipt(IFormFile imageFile)
    {
        if (imageFile is null || imageFile.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Image file is required." });

        using var stream = imageFile.OpenReadStream();
        var result = await Mediator.Send(new ProcessReceiptOcrCommand(UserId, stream, imageFile.ContentType));
        return HandleResult(result);
    }
}

[Authorize]
public sealed class SmsController : BaseApiController
{
    /// <summary>
    /// F5 — SMS Import: parse bank SMS string on Android.
    /// </summary>
    [HttpPost("parse")]
    [ProducesResponseType(typeof(CaptureConfirmationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ParseSms([FromBody] ParseSmsRequest request)
        => HandleResult(await Mediator.Send(new ParseSmsTransactionCommand(UserId, request.SmsBody, request.Sender)));
}

public sealed record ParseSmsRequest(string SmsBody, string Sender);

[Authorize]
public sealed class QrController : BaseApiController
{
    /// <summary>
    /// F4 — QR Scanner: parse QR code text payload.
    /// </summary>
    [HttpPost("parse")]
    [ProducesResponseType(typeof(CaptureConfirmationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ParseQr([FromBody] ParseQrRequest request)
        => HandleResult(await Mediator.Send(new ParseQrCodeCommand(UserId, request.QrContent)));
}

public sealed record ParseQrRequest(string QrContent);

[Authorize]
public sealed class DashboardController : BaseApiController
{
    /// <summary>
    /// F8 — Basic Dashboard: total spend, category breakdown, recent transactions.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
        => HandleResult(await Mediator.Send(new GetDashboardSummaryQuery(UserId, month, year)));
}

[Authorize]
public sealed class RecommendationsController : BaseApiController
{
    /// <summary>
    /// F9 — AI Alternatives Engine (Demo Scope): get suggested cheaper product alternative.
    /// </summary>
    [HttpGet("{transactionId:guid}")]
    [ProducesResponseType(typeof(AlternativeRecommendationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlternative(Guid transactionId)
        => HandleResult(await Mediator.Send(new GetAlternativeQuery(transactionId, UserId)));
}
