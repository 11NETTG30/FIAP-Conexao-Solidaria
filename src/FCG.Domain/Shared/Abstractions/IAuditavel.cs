namespace FCG.Domain.Shared.Abstractions;

public interface IAuditavel
{
    DateTime DataCriacao { get; }
    DateTime? DataAtualizacao { get; }
}