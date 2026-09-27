namespace ConexaoSolidaria.Infrastructure.Doacoes.Persistence;

// Tabela de controle de idempotência do doacoes-worker
public sealed class DoacaoProcessada
{
    public Guid IdDoacao { get; private set; }
    public DateTime ProcessadaEm { get; private set; }

    public DoacaoProcessada(Guid idDoacao)
    {
        IdDoacao = idDoacao;
        ProcessadaEm = DateTime.UtcNow;
    }

    // EF Core
    private DoacaoProcessada() { }
}
