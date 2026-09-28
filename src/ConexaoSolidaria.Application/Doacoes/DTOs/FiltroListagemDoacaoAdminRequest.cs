namespace ConexaoSolidaria.Application.Doacoes.DTOs;

public sealed record FiltroListagemDoacaoAdminRequest : FiltroListagemDoacaoRequest
{
    public Guid? IdDoador { get; init; }
}
