namespace ConexaoSolidaria.Application.Identidade.DTOs;

public record LogoutRequest(
    Guid RefreshToken
);