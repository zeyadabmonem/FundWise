using Moq;
using FundWise.Application.Common.Interfaces;
using FundWise.Application.Features.Authentication.Commands;
using FundWise.Domain.Entities;
using FundWise.Domain.Interfaces;
using Xunit;

namespace FundWise.UnitTests;

public class RegisterUserHandlerTests
{
    private readonly Mock<IRepository<User>> _usersMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokenMock = new();

    public RegisterUserHandlerTests()
    {
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed_secret_password");
        _tokenMock.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns("access_token");
        _tokenMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");
    }

    [Fact]
    public async Task Handle_Should_Create_User_When_Email_Is_Unique()
    {
        // Arrange
        _usersMock.Setup(u => u.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

        var handler = new RegisterUserHandler(_usersMock.Object, _uowMock.Object, _hasherMock.Object, _tokenMock.Object);
        var command = new RegisterUserCommand("Zeyad Abdo", "zeyad@fundwise.ai", "Password123!", "EGP");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("zeyad@fundwise.ai", result.Value.Profile.Email);
        Assert.Equal("EGP", result.Value.Profile.Currency);
        Assert.Equal("access_token", result.Value.Tokens.AccessToken);

        _usersMock.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Email_Exists()
    {
        // Arrange
        _usersMock.Setup(u => u.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        var handler = new RegisterUserHandler(_usersMock.Object, _uowMock.Object, _hasherMock.Object, _tokenMock.Object);
        var command = new RegisterUserCommand("Zeyad Abdo", "existing@fundwise.ai", "Password123!", "EGP");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.DuplicateEmail", result.Error.Code);
    }
}
