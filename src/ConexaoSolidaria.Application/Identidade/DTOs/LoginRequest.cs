namespace ConexaoSolidaria.Application.Identidade.DTOs;

public record LoginRequest(
    string Email,
    string Senha
);