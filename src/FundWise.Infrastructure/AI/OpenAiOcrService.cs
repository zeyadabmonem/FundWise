using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using FundWise.Application.Common.Interfaces;

namespace FundWise.Infrastructure.AI;

/// <summary>
/// F3 — Receipt OCR scanner using GPT-4o Vision.
/// </summary>
public sealed class OpenAiOcrService : IOcrService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OpenAiOcrService> _logger;

    public OpenAiOcrService(IConfiguration config, ILogger<OpenAiOcrService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<CapturedTransaction?> ProcessImageAsync(Stream imageStream, string mimeType, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured — returning mock receipt OCR.");
            return new CapturedTransaction(
                Merchant: "Carrefour Egypt (Mock OCR)",
                Amount: 450.50m,
                Currency: "EGP",
                CategoryHint: "Groceries",
                Date: DateTime.UtcNow,
                ConfidenceScore: 0.95,
                RawInput: "[Receipt Image: Carrefour Egypt - Total 450.50 EGP]");
        }

        try
        {
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms, cancellationToken);
            var bytes = ms.ToArray();
            var binaryData = BinaryData.FromBytes(bytes);

            var chatClient = new ChatClient("gpt-4o", apiKey);

            var systemPrompt = """
                You are a receipt OCR reader for Egyptian retail and restaurant receipts (in English or Arabic).
                Extract the merchant name, total grand amount (numeric), currency (default EGP), category_hint, and date.
                Respond STRICTLY in JSON format:
                {
                  "merchant": "string",
                  "amount": number,
                  "currency": "EGP",
                  "category_hint": "FoodAndDrink|Groceries|Transport|BillsAndUtilities|Shopping|Entertainment|Health|Education|Transfer|Other",
                  "confidence": 0.0-1.0
                }
                """;

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Please extract transaction details from this receipt photo."),
                    ChatMessageContentPart.CreateImagePart(binaryData, mimeType))
            };

            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            var json = completion.Content[0].Text;
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new CapturedTransaction(
                Merchant: root.GetProperty("merchant").GetString() ?? "Unknown Merchant",
                Amount: root.GetProperty("amount").GetDecimal(),
                Currency: root.TryGetProperty("currency", out var curr) ? curr.GetString() ?? "EGP" : "EGP",
                CategoryHint: root.TryGetProperty("category_hint", out var cat) ? cat.GetString() : null,
                Date: DateTime.UtcNow,
                ConfidenceScore: root.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.90,
                RawInput: "OCR processed via GPT-4o Vision");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process receipt image via GPT-4o Vision");
            return null;
        }
    }
}
