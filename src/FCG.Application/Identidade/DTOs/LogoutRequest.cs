namespace FCG.Application.Identidade.DTOs;

public record LogoutRequest(
    Guid RefreshToken
);