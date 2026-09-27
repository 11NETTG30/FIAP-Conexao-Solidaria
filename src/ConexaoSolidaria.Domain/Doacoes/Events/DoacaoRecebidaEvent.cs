namespace ConexaoSolidaria.Domain.Doacoes.Events;

public sealed record DoacaoRecebidaEvent(
    Guid IdDoacao,
    Guid IdCampanha,
    decimal ValorDoacao
);