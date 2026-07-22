using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Domain.Entities;
using FundWise.Domain.Interfaces;

namespace FundWise.Application.Features.Recommendations.Queries;

// ────────────────────────────────────────────────────────────────────────────
// Get Alternative for Transaction (F9 — mock data demo)
// ────────────────────────────────────────────────────────────────────────────

public sealed record GetAlternativeQuery(
    Guid TransactionId,
    Guid UserId) : IRequest<Result<AlternativeRecommendationDto?>>;

public sealed record AlternativeRecommendationDto(
    Guid TransactionId,
    string CurrentProductName,
    decimal CurrentPrice,
    string AlternativeName,
    string AlternativeBrand,
    decimal AlternativePrice,
    decimal SavingAmount,
    double SavingPercentage,
    decimal ProjectedMonthlySaving,
    decimal ProjectedYearlySaving,
    string Disclaimer = "ⓘ This is a suggested alternative based on example data. Prices may vary.");

public sealed class GetAlternativeHandler : IRequestHandler<GetAlternativeQuery, Result<AlternativeRecommendationDto?>>
{
    private readonly IRepository<Transaction> _transactions;
    private readonly IAlternativesService _alternatives;

    public GetAlternativeHandler(IRepository<Transaction> transactions, IAlternativesService alternatives)
    { _transactions = transactions; _alternatives = alternatives; }

    public async Task<Result<AlternativeRecommendationDto?>> Handle(GetAlternativeQuery request, CancellationToken cancellationToken)
    {
        var t = await _transactions.GetByIdAsync(request.TransactionId, cancellationToken);
        if (t is null || t.UserId != request.UserId)
            return Error.NotFound(nameof(Transaction), request.TransactionId);

        var recommendation = await _alternatives.GetAlternativeAsync(
            t.Merchant, null, t.Amount, t.Category, cancellationToken);

        if (recommendation is null)
            return (AlternativeRecommendationDto?)null; // No match — no card shown

        return new AlternativeRecommendationDto(
            t.Id,
            recommendation.CurrentProductName,
            recommendation.CurrentPrice,
            recommendation.AlternativeName,
            recommendation.AlternativeBrand,
            recommendation.AlternativePrice,
            recommendation.SavingAmount,
            recommendation.SavingPercentage,
            recommendation.ProjectedMonthlySaving,
            recommendation.ProjectedYearlySaving);
    }
}
