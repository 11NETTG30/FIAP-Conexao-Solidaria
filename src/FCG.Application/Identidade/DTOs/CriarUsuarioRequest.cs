namespace FCG.Application.Identidade.DTOs;

public record CriarUsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    string ConfirmacaoSenha
);