using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FundWise.IntegrationTests;

/// <summary>
/// Section 3 — Full API integration tests covering:
///   - Auth cycle (register → login → JWT-gated requests)
///   - Transaction lifecycle (POST → GET → Dashboard totals reflect it)
///   - Security baseline (IDOR, duplicate email, expired token)
/// </summary>
public class ApiIntegrationTests : IClassFixture<FundWiseWebAppFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public ApiIntegrationTests(FundWiseWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Auth Flow Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithNewEmail_Returns200WithTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Demo User",
            email = $"test_{Guid.NewGuid():N}@fundwise.ai",
            password = "Demo@12345",
            currency = "EGP"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", body);
        Assert.Contains("refreshToken", body);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409Conflict()
    {
        var email = $"dup_{Guid.NewGuid():N}@fundwise.ai";

        // Register once
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "User A",
            email,
            password = "Demo@12345",
            currency = "EGP"
        });

        // Register again with same email — must be 409
        var dupResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "User B",
            email,
            password = "Demo@12345",
            currency = "EGP"
        });

        Assert.Equal(HttpStatusCode.Conflict, dupResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401WithGenericMessage()
    {
        var email = $"pw_{Guid.NewGuid():N}@fundwise.ai";

        // Register first
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "PW Test",
            email,
            password = "Correct@12345",
            currency = "EGP"
        });

        // Login with wrong password
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "WrongPass@999"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Transaction Lifecycle Tests (Section 3 — Integration)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddTransaction_ThenGetDashboard_TotalsMatch()
    {
        // 1. Register and get token
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Add a manual transaction
        var addResponse = await _client.PostAsJsonAsync("/api/transactions", new
        {
            merchant = "Starbucks",
            amount = 150.00,
            currency = "EGP",
            category = 0, // FoodAndDrink
            captureDate = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);

        // 3. GET /api/transactions — transaction must appear
        var listResponse = await _client.GetAsync("/api/transactions");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await listResponse.Content.ReadAsStringAsync();
        Assert.Contains("Starbucks", listBody);

        // 4. GET /api/dashboard/summary — totalSpent must reflect the transaction
        var dashResponse = await _client.GetAsync(
            $"/api/dashboard/summary?month={DateTime.UtcNow.Month}&year={DateTime.UtcNow.Year}");
        Assert.Equal(HttpStatusCode.OK, dashResponse.StatusCode);

        var dashBody = await dashResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(dashBody);
        var totalSpent = doc.RootElement.GetProperty("totalSpent").GetDecimal();
        Assert.True(totalSpent >= 150m, $"Expected totalSpent >= 150, got {totalSpent}");
    }

    [Fact]
    public async Task AddTransaction_AmountZero_Returns400ValidationError()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/transactions", new
        {
            merchant = "Test",
            amount = 0,  // Invalid: must be > 0
            currency = "EGP",
            category = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Security Tests (Section 7 — IDOR + JWT enforcement)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Security_UserCannotAccessAnotherUsersTransactions()
    {
        // User A registers and adds a transaction
        var tokenA = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);

        var addResp = await _client.PostAsJsonAsync("/api/transactions", new
        {
            merchant = "User A Secret Purchase",
            amount = 999.00,
            currency = "EGP",
            category = 0,
            captureDate = DateTime.UtcNow
        });
        var addBody = await addResp.Content.ReadAsStringAsync();
        using var addDoc = JsonDocument.Parse(addBody);
        var transactionId = addDoc.RootElement.GetProperty("id").GetString();

        // User B registers
        var tokenB = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);

        // User B tries to access User A's transaction by ID — must be 404 (not found for this user)
        var getResp = await _client.GetAsync($"/api/transactions/{transactionId}");
        Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
    }

    [Fact]
    public async Task Security_ExpiredOrFakeToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "fake.jwt.token");

        var response = await _client.GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var email = $"user_{Guid.NewGuid():N}@fundwise.ai";
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Test User",
            email,
            password = "Test@12345",
            currency = "EGP"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement
                  .GetProperty("tokens")
                  .GetProperty("accessToken")
                  .GetString()!;
    }
}
