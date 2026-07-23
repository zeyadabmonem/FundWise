using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FundWise.Application.Common.Interfaces;
using FundWise.Persistence.DbContext;
using Moq;

namespace FundWise.IntegrationTests;

/// <summary>
/// Shared WebApplicationFactory that replaces EF Core with an in-memory DB
/// and mocks all external AI services so tests are fast, free, and deterministic.
/// </summary>
public class FundWiseWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"FundWise_Test_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "true",
                ["InMemoryDatabaseName"] = _dbName
            });
        });

        builder.ConfigureServices(services =>
        {
            // ── Mock IOcrService (F3) — no real OpenAI calls in tests ──────
            var ocrDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IOcrService));
            if (ocrDescriptor != null) services.Remove(ocrDescriptor);
            var ocrMock = new Mock<IOcrService>();
            ocrMock.Setup(s => s.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new CapturedTransaction(
                       Merchant: "Starbucks",
                       Amount: 65.00m,
                       Currency: "EGP",
                       CategoryHint: "FoodAndDrink",
                       Date: DateTime.UtcNow,
                       ConfidenceScore: 0.95,
                       RawInput: "receipt_image"));
            services.AddSingleton(ocrMock.Object);

            // ── Mock IVoiceService (F2) — no real Whisper calls in tests ───
            var voiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IVoiceService));
            if (voiceDescriptor != null) services.Remove(voiceDescriptor);
            var voiceMock = new Mock<IVoiceService>();
            voiceMock.Setup(s => s.ProcessAudioAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new CapturedTransaction(
                         Merchant: "Carrefour",
                         Amount: 120.00m,
                         Currency: "EGP",
                         CategoryHint: "Groceries",
                         Date: DateTime.UtcNow,
                         ConfidenceScore: 0.88,
                         RawInput: "voice_recording"));
            services.AddSingleton(voiceMock.Object);

            // ── Ensure DB is created for each test ─────────────────────────
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FundWiseDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development");
    }
}
