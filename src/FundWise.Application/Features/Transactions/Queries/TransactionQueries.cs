using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Domain.Entities;
using FundWise.Domain.Enums;
using FundWise.Domain.Interfaces;

namespace FundWise.Application.Features.Transactions.Queries;

// ────────────────────────────────────────────────────────────────────────────
// Get All Transactions (filtered, paginated)
// ────────────────────────────────────────────────────────────────────────────

public sealed record GetTransactionsQuery(
    Guid UserId,
    int? Month = null,
    int? Year = null,
    TransactionCategory? Category = null,
    CaptureSource? Source = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<PagedResult<TransactionSummaryDto>>>;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class GetTransactionsHandler : IRequestHandler<GetTransactionsQuery, Result<PagedResult<TransactionSummaryDto>>>
{
    private readonly IRepository<Transaction> _transactions;

    public GetTransactionsHandler(IRepository<Transaction> transactions) => _transactions = transactions;

    public async Task<Result<PagedResult<TransactionSummaryDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var month = request.Month ?? DateTime.UtcNow.Month;

        var all = await _transactions.FindAsync(
            t => t.UserId == request.UserId
              && t.CaptureDate.Year == year
              && t.CaptureDate.Month == month
              && (request.Category == null || t.Category == request.Category)
              && (request.Source == null || t.Source == request.Source),
            cancellationToken);

        var ordered = all.OrderByDescending(t => t.CaptureDate).ToList();
        var total = ordered.Count;
        var items = ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionSummaryDto(
                t.Id, t.Merchant, t.Amount, t.Currency,
                t.Category, t.Category.ToString(), t.Source,
                t.CaptureDate, t.IsConfirmed, t.NeedsReview))
            .ToList();

        return new PagedResult<TransactionSummaryDto>(items, total, request.Page, request.PageSize);
    }
}

// ────────────────────────────────────────────────────────────────────────────
// Get Transaction By Id
// ────────────────────────────────────────────────────────────────────────────

public sealed record GetTransactionByIdQuery(Guid TransactionId, Guid UserId) : IRequest<Result<TransactionDto>>;

public sealed class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionDto>>
{
    private readonly IRepository<Transaction> _transactions;

    public GetTransactionByIdHandler(IRepository<Transaction> transactions) => _transactions = transactions;

    public async Task<Result<TransactionDto>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await _transactions.GetByIdAsync(request.TransactionId, cancellationToken);
        if (t is null || t.UserId != request.UserId)
            return Error.NotFound(nameof(Transaction), request.TransactionId);

        return new TransactionDto(
            t.Id, t.UserId, t.Merchant, t.Amount, t.Currency,
            t.Category, t.Category.ToString(), t.Source, t.Source.ToString(),
            t.CaptureDate, t.Notes, t.IsConfirmed, t.NeedsReview, t.ConfidenceScore, t.CreatedAt);
    }
}
