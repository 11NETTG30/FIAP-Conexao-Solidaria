namespace FCG.Application.Identidade.DTOs;

public record AlterarSenhaRequest(
    string SenhaAtual,
    string NovaSenha,
    string ConfirmacaoNovaSenha
);