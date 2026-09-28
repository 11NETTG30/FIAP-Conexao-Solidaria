using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Repositories;

namespace ConexaoSolidaria.Application.Doacoes.UseCases;

public sealed class ListarDoacoesUseCase
{
    private readonly IDoacaoRepository _doacaoRepository;
    private readonly IInformacoesUsuarioLogado _informacoesUsuarioLogado;

    public ListarDoacoesUseCase
    (
        IDoacaoRepository doacaoRepository,
        IInformacoesUsuarioLogado informacoesUsuarioLogado
    )
    {
        _doacaoRepository = doacaoRepository;
        _informacoesUsuarioLogado = informacoesUsuarioLogado;
    }

    public async Task<PaginaDto<DoacaoDto>> Executar(FiltroListagemDoacaoRequest request)
    {
        FiltroListagemDoacao filtro = new(
            Status: request.Status,
            IdCampanha: request.IdCampanha,
            IdDoador: _informacoesUsuarioLogado.Id,
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