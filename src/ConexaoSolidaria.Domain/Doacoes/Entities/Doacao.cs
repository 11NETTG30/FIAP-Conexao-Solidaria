using ConexaoSolidaria.Domain.Doacoes.Enums;
using ConexaoSolidaria.Domain.Shared.Abstractions;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Domain.Doacoes.Entities;

public sealed class Doacao : Entity, IAggregateRoot, IAuditavel
{
    public Guid IdCampanha { get; private set; }
    public Guid IdDoador { get; private set; }
    public decimal ValorDoacao { get; private set; }
    public StatusDoacao Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public Doacao
    (
        Guid idCampanha,
        Guid idDoador,
        decimal valorDoacao
    )
    {
        SetIdCampanha(idCampanha);
        SetIdDoador(idDoador);
        SetValorDoacao(valorDoacao);

        Status = StatusDoacao.Pendente;
    }

    // EF Core
    private Doacao() { }

    public void SetIdCampanha(Guid idCampanha)
    {
        if (idCampanha == Guid.Empty)
            throw new ValidationException("Id da campanha é obrigatório");

        IdCampanha = idCampanha;
    }

    public void SetIdDoador(Guid idDoador)
    {
        if (idDoador == Guid.Empty)
            throw new ValidationException("Id do doador é obrigatório");

        IdDoador = idDoador;
    }

    public void SetValorDoacao(decimal valorDoacao)
    {
        if (valorDoacao <= 0)
            throw new ValidationException("Valor da doação deve ser maior que zero");

        if (decimal.Round(valorDoacao, 2) != valorDoacao)
            throw new ValidationException("Valor da doação deve ter no máximo 2 casas decimais");

        ValorDoacao = valorDoacao;
    }

    public void Confirmar()
    {
        GarantirQueEstaPendente();
        Status = StatusDoacao.Confirmada;
    }

    public void Rejeitar()
    {
        GarantirQueEstaPendente();
        Status = StatusDoacao.Rejeitada;
    }

    private void GarantirQueEstaPendente()
    {
        if (Status != StatusDoacao.Pendente)
            throw new ConflictException("Doação já foi processada");
    }

    public override string ToString() =>
        $"{Id} - {ValorDoacao:C} - {Status}";
}