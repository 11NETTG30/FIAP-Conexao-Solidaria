using ConexaoSolidaria.Domain.Doacoes.Enums;

namespace ConexaoSolidaria.Domain.Doacoes.Repositories;

public record FiltroListagemDoacao(
    StatusDoacao? Status,
    Guid? IdCampanha,
    Guid? IdDoador,
    DateTime? DataInicio,
    DateTime? DataFim,
    int Pagina,
    int TamanhoPagina
);
