using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FundWise.API.Middleware;
using FundWise.Application;
using FundWise.Infrastructure;
using FundWise.Persistence;
using FundWise.Persistence.DbContext;
using FundWise.Persistence.SeedData;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

// 2. Controllers & API configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. JWT Authentication setup
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "FundWiseSecretKey_MustBeAtLeast32BytesLong_SuperSecretKey2026!";
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Swagger / OpenAPI with JWT Authorization support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FundWise AI API",
        Version = "v1",
        Description = "FundWise AI (FinPilot) Backend API — Phase 1 MVP Scope (Clean Architecture)"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 5. CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 6. Database auto-migrate & seed on startup (development environment)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FundWiseDbContext>();
        dbContext.Database.EnsureCreated();
        await AlternativeProductSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// 7. HTTP request pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FundWise AI API v1");
        c.RoutePrefix = "swagger";
    });

    // Redirect root URL (/) to /swagger for easy browser access
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseStaticFiles(); // For receipt uploaded images
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible to integration test WebApplicationFactory
public partial class Program { }
