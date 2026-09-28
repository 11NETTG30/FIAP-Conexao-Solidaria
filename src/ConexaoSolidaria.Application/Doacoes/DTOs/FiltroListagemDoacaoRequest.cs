using ConexaoSolidaria.Domain.Doacoes.Enums;

namespace ConexaoSolidaria.Application.Doacoes.DTOs;

public record FiltroListagemDoacaoRequest
{
    public StatusDoacao? Status { get; init; }
    public Guid? IdCampanha { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanhoPagina { get; init; } = 20;
}
