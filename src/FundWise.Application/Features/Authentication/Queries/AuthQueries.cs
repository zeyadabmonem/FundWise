using MediatR;
using FundWise.Application.Common;
using FundWise.Application.Features.Authentication.DTOs;
using FundWise.Domain.Entities;
using FundWise.Domain.Interfaces;

namespace FundWise.Application.Features.Authentication.Queries;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
{
    private readonly IRepository<User> _users;

    public GetCurrentUserHandler(IRepository<User> users) => _users = users;

    public async Task<Result<UserProfileDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Error.NotFound(nameof(User), request.UserId);

        return new UserProfileDto(user.Id, user.Name, user.Email, user.Currency, user.CreatedAt);
    }
}

// ────────────────────────────────────────────────────────────────────────────
// Update Profile
// ────────────────────────────────────────────────────────────────────────────
public sealed record UpdateUserProfileCommand(Guid UserId, string? Name, string? Currency) : IRequest<Result<UserProfileDto>>;

public sealed class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _uow;

    public UpdateUserProfileHandler(IRepository<User> users, IUnitOfWork uow) { _users = users; _uow = uow; }

    public async Task<Result<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return Error.NotFound(nameof(User), request.UserId);

        user.UpdateProfile(request.Name, request.Currency);
        _users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return new UserProfileDto(user.Id, user.Name, user.Email, user.Currency, user.CreatedAt);
    }
}
