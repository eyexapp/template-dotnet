using MyApp.Application.Auth;
using MyApp.Application.Auth.DTOs;

namespace Microsoft.AspNetCore.Builder;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .AllowAnonymous();

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService, CancellationToken ct) =>
        {
            var result = await authService.RegisterAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .WithName("Register")
        .WithDescription("Register a new user account")
        .Produces<AuthResponse>()
        .ProducesProblem(400);

        group.MapPost("/login", async (LoginRequest request, IAuthService authService, CancellationToken ct) =>
        {
            var result = await authService.LoginAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .WithName("Login")
        .WithDescription("Authenticate and receive tokens")
        .Produces<AuthResponse>()
        .ProducesProblem(401);

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService, CancellationToken ct) =>
        {
            var result = await authService.RefreshTokenAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: result.StatusCode);
        })
        .WithName("RefreshToken")
        .WithDescription("Refresh access token using refresh token")
        .Produces<AuthResponse>()
        .ProducesProblem(401);
    }
}
