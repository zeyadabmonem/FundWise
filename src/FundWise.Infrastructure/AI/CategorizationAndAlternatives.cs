using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using FundWise.Application.Common.Interfaces;
using FundWise.Domain.Entities;
using FundWise.Domain.Enums;
using FundWise.Domain.Interfaces;

namespace FundWise.Infrastructure.AI;

/// <summary>
/// F7 — AI Expense Categorization service using GPT-4o with rule fallback.
/// </summary>
public sealed class OpenAiCategorizationService : ICategorizationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OpenAiCategorizationService> _logger;

    public OpenAiCategorizationService(IConfiguration config, ILogger<OpenAiCategorizationService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<(TransactionCategory Category, double Confidence)> CategorizeAsync(
        string merchant,
        decimal amount,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        // Rule-based fast paths for Egyptian market common merchants
        var fastCategory = GetFastCategory(merchant);
        if (fastCategory.HasValue)
            return (fastCategory.Value, 0.95);

        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (TransactionCategory.Other, 0.60);
        }

        try
        {
            var chatClient = new ChatClient("gpt-4o", apiKey);
            var systemPrompt = """
                Categorize this transaction into exactly ONE of the following category strings:
                FoodAndDrink, Groceries, Transport, BillsAndUtilities, Shopping, Entertainment, Health, Education, Transfer, Other.
                Respond strictly in JSON format: {"category": "CategoryString", "confidence": 0.0-1.0}
                """;

            var userPrompt = $"Merchant: {merchant}, Amount: {amount} EGP, Context: {description ?? "N/A"}";

            ChatCompletion completion = await chatClient.CompleteChatAsync(
                [new SystemChatMessage(systemPrompt), new UserChatMessage(userPrompt)],
                cancellationToken: cancellationToken);

            var json = completion.Content[0].Text;
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var catString = root.GetProperty("category").GetString();
            var confidence = root.GetProperty("confidence").GetDouble();

            if (Enum.TryParse<TransactionCategory>(catString, true, out var category))
            {
                return (category, confidence);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI categorization failed for merchant {Merchant}", merchant);
        }

        return (TransactionCategory.Other, 0.50);
    }

    private static TransactionCategory? GetFastCategory(string merchant)
    {
        var lower = merchant.ToLowerInvariant();
        if (lower.Contains("starbucks") || lower.Contains("costa") || lower.Contains("mcdonald") || lower.Contains("kfc") || lower.Contains("tbk") || lower.Contains("el prince") || lower.Contains("restaurant") || lower.Contains("café") || lower.Contains("cafe"))
            return TransactionCategory.FoodAndDrink;
        if (lower.Contains("carrefour") || lower.Contains("spinneys") || lower.Contains("hyper one") || lower.Contains("seoudi") || lower.Contains("metro") || lower.Contains("kazyon") || lower.Contains("bimarco"))
            return TransactionCategory.Groceries;
        if (lower.Contains("uber") || lower.Contains("indrive") || lower.Contains("diDi") || lower.Contains("careem") || lower.Contains("swvl") || lower.Contains("gasoline") || lower.Contains("petromin"))
            return TransactionCategory.Transport;
        if (lower.Contains("vodafone") || lower.Contains("orange") || lower.Contains("etisalat") || lower.Contains("we ") || lower.Contains("fawry") || lower.Contains("electricity") || lower.Contains("gas"))
            return TransactionCategory.BillsAndUtilities;
        if (lower.Contains("instapay") || lower.Contains("cib transfer") || lower.Contains("bank transfer"))
            return TransactionCategory.Transfer;

        return null;
    }
}

/// <summary>
/// F9 — AI Alternatives Engine using SQL Server mock dataset lookup.
/// </summary>
public sealed class OpenAiAlternativesService : IAlternativesService
{
    private readonly IRepository<AlternativeProduct> _alternativesRepo;

    public OpenAiAlternativesService(IRepository<AlternativeProduct> alternativesRepo)
    {
        _alternativesRepo = alternativesRepo;
    }

    public async Task<AlternativeRecommendation?> GetAlternativeAsync(
        string merchant,
        string? productName,
        decimal amount,
        TransactionCategory category,
        CancellationToken cancellationToken = default)
    {
        var allAlternatives = await _alternativesRepo.GetAllAsync(cancellationToken);
        if (!allAlternatives.Any()) return null;

        var merchantLower = merchant.ToLowerInvariant();
        var searchName = (productName ?? merchant).ToLowerInvariant();

        // 1. Try exact product/brand match
        var match = allAlternatives.FirstOrDefault(a =>
            searchName.Contains(a.ProductName.ToLowerInvariant()) ||
            searchName.Contains(a.ProductBrand.ToLowerInvariant()));

        // 2. Fall back to category match with closest price
        match ??= allAlternatives
            .Where(a => a.Category == category && a.CurrentPrice <= amount * 1.3m)
            .OrderBy(a => Math.Abs(a.CurrentPrice - amount))
            .FirstOrDefault();

        if (match is null) return null;

        var monthlySaving = match.SavingAmount * 4; // assume 4x purchases/month
        var yearlySaving = monthlySaving * 12;

        return new AlternativeRecommendation(
            CurrentProductName: $"{match.ProductBrand} {match.ProductName}",
            CurrentPrice: match.CurrentPrice,
            AlternativeName: match.AlternativeName,
            AlternativeBrand: match.AlternativeBrand,
            AlternativePrice: match.AlternativePrice,
            SavingAmount: match.SavingAmount,
            SavingPercentage: match.SavingPercentage,
            ProjectedMonthlySaving: monthlySaving,
            ProjectedYearlySaving: yearlySaving);
    }
}
