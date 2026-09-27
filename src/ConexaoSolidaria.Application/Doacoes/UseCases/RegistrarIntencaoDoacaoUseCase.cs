using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Repositories;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Events;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Application.Doacoes.UseCases;

public sealed class RegistrarIntencaoDoacaoUseCase
{
    private readonly IDoacaoRepository _doacaoRepository;
    private readonly ICampanhaRepository _campanhaRepository;
    private readonly IInformacoesUsuarioLogado _informacoesUsuarioLogado;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarIntencaoDoacaoUseCase
    (
        IDoacaoRepository doacaoRepository,
        ICampanhaRepository campanhaRepository,
        IInformacoesUsuarioLogado informacoesUsuarioLogado,
        IEventPublisher eventPublisher
    )
    {
        _doacaoRepository = doacaoRepository;
        _campanhaRepository = campanhaRepository;
        _informacoesUsuarioLogado = informacoesUsuarioLogado;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> Executar(CriarDoacaoRequest request)
    {
        Campanha campanha = await _campanhaRepository.ObterPorId(request.IdCampanha)
            ?? throw new NotFoundException("Campanha não encontrada");

        if (!campanha.PodeReceberDoacao())
            throw new ConflictException("Campanha não está apta a receber doações no momento");

        Doacao doacao = new(request.IdCampanha, _informacoesUsuarioLogado.Id, request.ValorDoacao);

        await _doacaoRepository.Adicionar(doacao);
        await _eventPublisher.Publicar(new DoacaoRecebidaEvent(doacao.Id, doacao.IdCampanha, doacao.ValorDoacao));
        await _doacaoRepository.UnitOfWork.Commit();

        return doacao.Id;
    }
}
