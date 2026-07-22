using FluentValidation;
using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Common.Interfaces;
using FundWise.Application.Features.Authentication.DTOs;
using FundWise.Domain.Entities;
using FundWise.Domain.Interfaces;

namespace FundWise.Application.Features.Authentication.Commands;

// ────────────────────────────────────────────────────────────────────────────
// Register
// ────────────────────────────────────────────────────────────────────────────

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Password,
    string Currency = "EGP") : IRequest<Result<LoginResponseDto>>;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter code (e.g. EGP).");
    }
}

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<LoginResponseDto>>
{
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterUserHandler(
        IRepository<User> users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponseDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var exists = await _users.ExistsAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);
        if (exists)
            return Error.Conflict("Auth.DuplicateEmail", "An account with this email already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash, request.Currency);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Name);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(30);

        user.SetRefreshToken(refreshToken, expiry);

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto(
            new AuthTokenDto(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(60)),
            new UserProfileDto(user.Id, user.Name, user.Email, user.Currency, user.CreatedAt));
    }
}

// ────────────────────────────────────────────────────────────────────────────
// Login
// ────────────────────────────────────────────────────────────────────────────

public sealed record LoginUserCommand(
    string Email,
    string Password) : IRequest<Result<LoginResponseDto>>;

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<LoginResponseDto>>
{
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginUserHandler(
        IRepository<User> users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        // Intentionally generic error to prevent user enumeration
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Error.Unauthorized("Invalid credentials.");

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Name);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(30));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto(
            new AuthTokenDto(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(60)),
            new UserProfileDto(user.Id, user.Name, user.Email, user.Currency, user.CreatedAt));
    }
}

// ────────────────────────────────────────────────────────────────────────────
// Refresh Token
// ────────────────────────────────────────────────────────────────────────────

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<Result<AuthTokenDto>>;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RefreshTokenHandler(IRepository<User> users, IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user is null || !user.IsRefreshTokenValid(request.RefreshToken))
            return Error.Unauthorized("Invalid or expired refresh token.");

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Name);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(30));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenDto(accessToken, newRefreshToken, DateTime.UtcNow.AddMinutes(60));
    }
}

// ────────────────────────────────────────────────────────────────────────────
// Logout
// ────────────────────────────────────────────────────────────────────────────

public sealed record LogoutUserCommand(Guid UserId) : IRequest<Result>;

public sealed class LogoutUserHandler : IRequestHandler<LogoutUserCommand, Result>
{
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutUserHandler(IRepository<User> users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return Result.Success;

        user.RevokeRefreshToken();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
