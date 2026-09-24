namespace FCG.Application.Identidade.DTOs;

public record RefreshRequest(
    Guid RefreshToken
);