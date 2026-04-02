namespace MyApp.Application.Auth.DTOs;

public record RegisterRequest(string FullName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
