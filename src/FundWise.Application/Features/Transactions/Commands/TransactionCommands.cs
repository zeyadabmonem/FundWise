using FluentValidation;
using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Application.Features.Transactions.DTOs;
using FundWise.Domain.Entities;
using FundWise.Domain.Enums;
using FundWise.Domain.Interfaces;

namespace FundWise.Application.Features.Transactions.Commands;

// ────────────────────────────────────────────────────────────────────────────
// Create Transaction (used internally by all capture pipelines)
// ────────────────────────────────────────────────────────────────────────────

public sealed record CreateTransactionCommand(
    Guid UserId,
    string Merchant,
    decimal Amount,
    string Currency,
    TransactionCategory? Category,
    CaptureSource Source,
    DateTime? CaptureDate,
    string? Notes,
    bool IsConfirmed,
    double ConfidenceScore = 1.0) : IRequest<Result<TransactionDto>>;

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Result<TransactionDto>>
{
    private readonly IRepository<Transaction> _transactions;
    private readonly IRepository<MerchantCategoryMemory> _memories;
    private readonly IUnitOfWork _uow;
    private readonly ICategorizationService _categorization;

    public CreateTransactionHandler(
        IRepository<Transaction> transactions,
        IRepository<MerchantCategoryMemory> memories,
        IUnitOfWork uow,
        ICategorizationService categorization)
    {
        _transactions = transactions;
        _memories = memories;
        _uow = uow;
        _categorization = categorization;
    }

    public async Task<Result<TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        // 1. Check per-user merchant memory first (user corrections take priority)
        var memoryKey = request.Merchant.Trim().ToLowerInvariant();
        var memory = await _memories.FirstOrDefaultAsync(
            m => m.UserId == request.UserId && m.MerchantKey == memoryKey,
            cancellationToken);

        TransactionCategory category;
        double confidence;

        if (memory is not null)
        {
            category = memory.Category;
            confidence = 1.0;
        }
        else if (request.Category.HasValue)
        {
            category = request.Category.Value;
            confidence = request.ConfidenceScore;
        }
        else
        {
            // 2. AI categorization
            (category, confidence) = await _categorization.CategorizeAsync(
                request.Merchant, request.Amount, request.Notes, cancellationToken);
        }

        var transaction = Transaction.Create(
            userId: request.UserId,
            merchant: request.Merchant,
            amount: request.Amount,
            currency: request.Currency,
            category: category,
            source: request.Source,
            captureDate: request.CaptureDate ?? DateTime.UtcNow,
            confidenceScore: confidence,
            notes: request.Notes,
            isConfirmed: request.IsConfirmed);

        await _transactions.AddAsync(transaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return MapToDto(transaction);
    }

    private static TransactionDto MapToDto(Transaction t) => new(
        t.Id, t.UserId, t.Merchant, t.Amount, t.Currency,
        t.Category, t.Category.ToString(), t.Source, t.Source.ToString(),
        t.CaptureDate, t.Notes, t.IsConfirmed, t.NeedsReview, t.ConfidenceScore, t.CreatedAt);
}

// ────────────────────────────────────────────────────────────────────────────
// Confirm Transaction (user approves auto-captured transaction)
// ────────────────────────────────────────────────────────────────────────────

public sealed record ConfirmTransactionCommand(Guid TransactionId, Guid UserId) : IRequest<Result<TransactionDto>>;

public sealed class ConfirmTransactionHandler : IRequestHandler<ConfirmTransactionCommand, Result<TransactionDto>>
{
    private readonly IRepository<Transaction> _transactions;
    private readonly IUnitOfWork _uow;

    public ConfirmTransactionHandler(IRepository<Transaction> transactions, IUnitOfWork uow)
    { _transactions = transactions; _uow = uow; }

    public async Task<Result<TransactionDto>> Handle(ConfirmTransactionCommand request, CancellationToken cancellationToken)
    {
        var t = await _transactions.GetByIdAsync(request.TransactionId, cancellationToken);
        if (t is null || t.UserId != request.UserId)
            return Error.NotFound(nameof(Transaction), request.TransactionId);

        t.Confirm();
        _transactions.Update(t);
        await _uow.SaveChangesAsync(cancellationToken);

        return MapToDto(t);
    }

    private static TransactionDto MapToDto(Transaction t) => new(
        t.Id, t.UserId, t.Merchant, t.Amount, t.Currency,
        t.Category, t.Category.ToString(), t.Source, t.Source.ToString(),
        t.CaptureDate, t.Notes, t.IsConfirmed, t.NeedsReview, t.ConfidenceScore, t.CreatedAt);
}

// ────────────────────────────────────────────────────────────────────────────
// Update Transaction (edit details + re-categorize)
// ────────────────────────────────────────────────────────────────────────────

public sealed record UpdateTransactionCommand(
    Guid TransactionId,
    Guid UserId,
    string? Merchant,
    decimal? Amount,
    TransactionCategory? Category,
    DateTime? CaptureDate,
    string? Notes) : IRequest<Result<TransactionDto>>;

public sealed class UpdateTransactionHandler : IRequestHandler<UpdateTransactionCommand, Result<TransactionDto>>
{
    private readonly IRepository<Transaction> _transactions;
    private readonly IRepository<MerchantCategoryMemory> _memories;
    private readonly IUnitOfWork _uow;

    public UpdateTransactionHandler(
        IRepository<Transaction> transactions,
        IRepository<MerchantCategoryMemory> memories,
        IUnitOfWork uow)
    { _transactions = transactions; _memories = memories; _uow = uow; }

    public async Task<Result<TransactionDto>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var t = await _transactions.GetByIdAsync(request.TransactionId, cancellationToken);
        if (t is null || t.UserId != request.UserId)
            return Error.NotFound(nameof(Transaction), request.TransactionId);

        // If category was corrected, update merchant memory
        if (request.Category.HasValue && request.Category != t.Category)
        {
            var merchantKey = (request.Merchant ?? t.Merchant).Trim().ToLowerInvariant();
            var memory = await _memories.FirstOrDefaultAsync(
                m => m.UserId == request.UserId && m.MerchantKey == merchantKey, cancellationToken);

            if (memory is null)
                await _memories.AddAsync(
                    MerchantCategoryMemory.Create(request.UserId, request.Merchant ?? t.Merchant, request.Category.Value),
                    cancellationToken);
            else
            {
                memory.UpdateCategory(request.Category.Value);
                _memories.Update(memory);
            }
        }

        t.UpdateDetails(request.Merchant, request.Amount, request.Category, request.CaptureDate, request.Notes);
        _transactions.Update(t);
        await _uow.SaveChangesAsync(cancellationToken);

        return MapToDto(t);
    }

    private static TransactionDto MapToDto(Transaction t) => new(
        t.Id, t.UserId, t.Merchant, t.Amount, t.Currency,
        t.Category, t.Category.ToString(), t.Source, t.Source.ToString(),
        t.CaptureDate, t.Notes, t.IsConfirmed, t.NeedsReview, t.ConfidenceScore, t.CreatedAt);
}

// ────────────────────────────────────────────────────────────────────────────
// Delete Transaction
// ────────────────────────────────────────────────────────────────────────────

public sealed record DeleteTransactionCommand(Guid TransactionId, Guid UserId) : IRequest<Result>;

public sealed class DeleteTransactionHandler : IRequestHandler<DeleteTransactionCommand, Result>
{
    private readonly IRepository<Transaction> _transactions;
    private readonly IUnitOfWork _uow;

    public DeleteTransactionHandler(IRepository<Transaction> transactions, IUnitOfWork uow)
    { _transactions = transactions; _uow = uow; }

    public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var t = await _transactions.GetByIdAsync(request.TransactionId, cancellationToken);
        if (t is null || t.UserId != request.UserId)
            return Error.NotFound(nameof(Transaction), request.TransactionId);

        _transactions.Remove(t);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
