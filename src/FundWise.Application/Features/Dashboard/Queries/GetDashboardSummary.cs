using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Domain.Entities;
using FundWise.Domain.Enums;
using FundWise.Domain.Interfaces;

namespace FundWise.Application.Features.Dashboard.Queries;

// ────────────────────────────────────────────────────────────────────────────
// Dashboard DTOs
// ────────────────────────────────────────────────────────────────────────────

public sealed record DashboardSummaryDto(
    decimal TotalSpent,
    string Currency,
    int TransactionCount,
    int Month,
    int Year,
    IReadOnlyList<CategoryBreakdownDto> CategoryBreakdown,
    IReadOnlyList<RecentTransactionDto> RecentTransactions);

public sealed record CategoryBreakdownDto(
    TransactionCategory Category,
    string CategoryName,
    decimal Total,
    int Count,
    double Percentage);

public sealed record RecentTransactionDto(
    Guid Id,
    string Merchant,
    decimal Amount,
    string Currency,
    TransactionCategory Category,
    string CategoryName,
    string SourceName,
    DateTime CaptureDate,
    bool IsConfirmed,
    bool NeedsReview);

// ────────────────────────────────────────────────────────────────────────────
// Get Dashboard Summary (F8)
// ────────────────────────────────────────────────────────────────────────────

public sealed record GetDashboardSummaryQuery(
    Guid UserId,
    int? Month = null,
    int? Year = null) : IRequest<Result<DashboardSummaryDto>>;

public sealed class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IRepository<Transaction> _transactions;
    private readonly IRepository<User> _users;

    public GetDashboardSummaryHandler(IRepository<Transaction> transactions, IRepository<User> users)
    { _transactions = transactions; _users = users; }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return Error.NotFound(nameof(User), request.UserId);

        var year = request.Year ?? DateTime.UtcNow.Year;
        var month = request.Month ?? DateTime.UtcNow.Month;

        var transactions = await _transactions.FindAsync(
            t => t.UserId == request.UserId
              && t.CaptureDate.Year == year
              && t.CaptureDate.Month == month,
            cancellationToken);

        var totalSpent = transactions.Sum(t => t.Amount);

        var categoryBreakdown = transactions
            .GroupBy(t => t.Category)
            .Select(g => new CategoryBreakdownDto(
                g.Key,
                g.Key.ToString(),
                g.Sum(t => t.Amount),
                g.Count(),
                totalSpent > 0 ? Math.Round((double)(g.Sum(t => t.Amount) / totalSpent) * 100, 1) : 0))
            .OrderByDescending(c => c.Total)
            .ToList();

        var recent = transactions
            .OrderByDescending(t => t.CaptureDate)
            .Take(10)
            .Select(t => new RecentTransactionDto(
                t.Id, t.Merchant, t.Amount, t.Currency,
                t.Category, t.Category.ToString(), t.Source.ToString(),
                t.CaptureDate, t.IsConfirmed, t.NeedsReview))
            .ToList();

        return new DashboardSummaryDto(
            totalSpent,
            user.Currency,
            transactions.Count,
            month,
            year,
            categoryBreakdown,
            recent);
    }
}
