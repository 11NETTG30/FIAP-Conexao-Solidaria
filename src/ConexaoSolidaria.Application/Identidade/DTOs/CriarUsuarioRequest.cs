namespace ConexaoSolidaria.Application.Identidade.DTOs;

public record CriarUsuarioRequest(
    string Nome,
    string Email,
    string Cpf,
    string Senha,
    string ConfirmacaoSenha
);