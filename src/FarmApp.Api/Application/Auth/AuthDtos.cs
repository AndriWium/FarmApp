namespace FarmApp.Api.Application.Auth;

public record LoginRequest(string UserName, string Password);
public record RefreshRequest(string RefreshToken);
public record TokenResponse(string AccessToken, string RefreshToken);
