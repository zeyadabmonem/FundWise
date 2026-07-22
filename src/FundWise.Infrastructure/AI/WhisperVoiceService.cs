using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Audio;
using FundWise.Application.Common.Interfaces;

namespace FundWise.Infrastructure.AI;

/// <summary>
/// F2 — Speech-to-text using OpenAI Whisper + GPT-4o structured extraction.
/// </summary>
public sealed class WhisperVoiceService : IVoiceService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WhisperVoiceService> _logger;

    public WhisperVoiceService(IConfiguration config, ILogger<WhisperVoiceService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<CapturedTransaction?> ProcessAudioAsync(Stream audioStream, string mimeType, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured — returning mock voice extraction.");
            return new CapturedTransaction(
                Merchant: "Starbucks (Mock Voice)",
                Amount: 120.00m,
                Currency: "EGP",
                CategoryHint: "FoodAndDrink",
                Date: DateTime.UtcNow,
                ConfidenceScore: 0.92,
                RawInput: "اشتريت قهوة من ستارباكس ب ١٢٠ جنيه");
        }

        try
        {
            // 1. Transcribe audio with Whisper
            var audioClient = new AudioClient("whisper-1", apiKey);
            var extension = mimeType.Contains("wav") ? ".wav" : mimeType.Contains("m4a") ? ".m4a" : ".mp3";
            var transcription = await audioClient.TranscribeAudioAsync(audioStream, $"audio{extension}", cancellationToken: cancellationToken);
            var rawText = transcription.Value.Text;

            if (string.IsNullOrWhiteSpace(rawText)) return null;

            // 2. Structured extraction with GPT-4o
            var chatClient = new ChatClient("gpt-4o", apiKey);
            var systemPrompt = """
                You are a financial transaction parser. Extract merchant, amount (numeric decimal), currency (default EGP), category_hint, and date (YYYY-MM-DD) from the voice transcript.
                Respond STRICTLY in JSON format:
                {
                  "merchant": "string",
                  "amount": number,
                  "currency": "EGP",
                  "category_hint": "FoodAndDrink|Groceries|Transport|BillsAndUtilities|Shopping|Entertainment|Health|Education|Transfer|Other",
                  "confidence": 0.0-1.0
                }
                """;

            ChatCompletion completion = await chatClient.CompleteChatAsync(
                [new SystemChatMessage(systemPrompt), new UserChatMessage(rawText)],
                cancellationToken: cancellationToken);

            var json = completion.Content[0].Text;
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new CapturedTransaction(
                Merchant: root.GetProperty("merchant").GetString() ?? "Unknown",
                Amount: root.GetProperty("amount").GetDecimal(),
                Currency: root.TryGetProperty("currency", out var curr) ? curr.GetString() ?? "EGP" : "EGP",
                CategoryHint: root.TryGetProperty("category_hint", out var cat) ? cat.GetString() : null,
                Date: DateTime.UtcNow,
                ConfidenceScore: root.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.85,
                RawInput: rawText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process voice capture via OpenAI");
            return null;
        }
    }
}
