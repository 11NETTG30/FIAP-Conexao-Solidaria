using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Repositories;

namespace ConexaoSolidaria.Application.Doacoes.UseCases;

public sealed class ListarDoacoesAdminUseCase
{
    private readonly IDoacaoRepository _doacaoRepository;

    public ListarDoacoesAdminUseCase
    (
        IDoacaoRepository doacaoRepository
    )
    {
        _doacaoRepository = doacaoRepository;
    }

    public async Task<PaginaDto<DoacaoDto>> Executar(FiltroListagemDoacaoAdminRequest request)
    {
        FiltroListagemDoacao filtro = new(
            Status: request.Status,
            IdCampanha: request.IdCampanha,
            IdDoador: request.IdDoador,
            DataInicio: request.DataInicio,
            DataFim: request.DataFim,
            Pagina: request.Pagina,
            TamanhoPagina: request.TamanhoPagina
        );

        ResultadoPaginado<Doacao> resultado = await _doacaoRepository.Listar(filtro);

        return new PaginaDto<DoacaoDto>(
            Itens: resultado.Itens.Select(doacao => (DoacaoDto)doacao),
            Pagina: request.Pagina,
            TamanhoPagina: request.TamanhoPagina,
            TotalItens: resultado.TotalItens
        );
    }
}
