namespace FundWise.Application.Features.Authentication.DTOs;

public sealed record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

public sealed record UserProfileDto(
    Guid Id,
    string Name,
    string Email,
    string Currency,
    DateTime CreatedAt);

public sealed record LoginResponseDto(
    AuthTokenDto Tokens,
    UserProfileDto Profile);
