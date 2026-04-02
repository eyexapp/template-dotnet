using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MyApp.Application.Auth;
using MyApp.Application.Auth.DTOs;
using MyApp.Application.Common;
using MyApp.Domain.Common;
using MyApp.Domain.Entities;
using MyApp.Domain.Repositories;
using MyApp.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace MyApp.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IValidator<RegisterRequest> _registerValidator = Substitute.For<IValidator<RegisterRequest>>();
    private readonly IValidator<LoginRequest> _loginValidator = Substitute.For<IValidator<LoginRequest>>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var settings = Options.Create(new JwtSettings
        {
            SecretKey = "test-secret-key-at-least-32-characters-long!!",
            Issuer = "Test",
            Audience = "Test",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7,
        });

        _sut = new AuthService(
            _userRepository,
            _unitOfWork,
            _jwtTokenService,
            settings,
            _registerValidator,
            _loginValidator);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest("Test User", "test@example.com", "Password1");

        _registerValidator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _jwtTokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh-token");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var request = new RegisterRequest("Test User", "existing@example.com", "Password1");

        _registerValidator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new User { Email = "existing@example.com" });

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "wrong-password");

        _loginValidator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }
}
