using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Application.Doacoes.UseCases;

public sealed class ObterDoacaoPorIdUseCase
{
    private readonly IDoacaoRepository _doacaoRepository;
    private readonly IInformacoesUsuarioLogado _informacoesUsuarioLogado;

    public ObterDoacaoPorIdUseCase
    (
        IDoacaoRepository doacaoRepository,
        IInformacoesUsuarioLogado informacoesUsuarioLogado
    )
    {
        _doacaoRepository = doacaoRepository;
        _informacoesUsuarioLogado = informacoesUsuarioLogado;
    }

    // Só o doador dono da doação pode consultar — tratado como "não
    // encontrado" para quem não é o dono, em vez de 403, para não revelar a
    // existência da doação de outro doador.
    public async Task<DoacaoDto> Executar(Guid id)
    {
        Doacao? doacao = await _doacaoRepository.ObterPorId(id);

        if (doacao is null || doacao.IdDoador != _informacoesUsuarioLogado.Id)
            throw new NotFoundException("Doação não encontrada");

        return (DoacaoDto)doacao;
    }
}
