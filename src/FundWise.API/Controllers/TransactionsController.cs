using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FundWise.Application.Features.Transactions.Commands;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Application.Features.Transactions.Queries;
using FundWise.Domain.Enums;

namespace FundWise.API.Controllers;

[Authorize]
public sealed class TransactionsController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] TransactionCategory? category,
        [FromQuery] CaptureSource? source,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => HandleResult(await Mediator.Send(new GetTransactionsQuery(UserId, month, year, category, source, page, pageSize)));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetTransactionByIdQuery(id, UserId)));

    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateManualTransactionRequest request)
    {
        var result = await Mediator.Send(new CreateTransactionCommand(
            UserId: UserId,
            Merchant: request.Merchant,
            Amount: request.Amount,
            Currency: request.Currency ?? "EGP",
            Category: request.Category,
            Source: CaptureSource.Manual,
            CaptureDate: request.CaptureDate,
            Notes: request.Notes,
            IsConfirmed: true));

        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}/confirm")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirm(Guid id)
        => HandleResult(await Mediator.Send(new ConfirmTransactionCommand(id, UserId)));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
        => HandleResult(await Mediator.Send(new UpdateTransactionCommand(
            id, UserId, request.Merchant, request.Amount, request.Category, request.CaptureDate, request.Notes)));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteTransactionCommand(id, UserId)));
}

public sealed record CreateManualTransactionRequest(
    string Merchant,
    decimal Amount,
    string? Currency,
    TransactionCategory? Category,
    DateTime? CaptureDate,
    string? Notes);

public sealed record UpdateTransactionRequest(
    string? Merchant,
    decimal? Amount,
    TransactionCategory? Category,
    DateTime? CaptureDate,
    string? Notes);
