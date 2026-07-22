using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FundWise.Application.Common.Interfaces;
using FundWise.Infrastructure.AI;
using FundWise.Infrastructure.Services;
using FundWise.Infrastructure.SMS;

namespace FundWise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Security & Storage
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // AI Services
        services.AddScoped<IVoiceService, WhisperVoiceService>();
        services.AddScoped<IOcrService, OpenAiOcrService>();
        services.AddScoped<ICategorizationService, OpenAiCategorizationService>();
        services.AddScoped<IAlternativesService, OpenAiAlternativesService>();

        // Capture Parsers
        services.AddScoped<ISmsParser, RegexSmsParser>();
        services.AddScoped<IQrParser, QrContentParser>();

        return services;
    }
}
