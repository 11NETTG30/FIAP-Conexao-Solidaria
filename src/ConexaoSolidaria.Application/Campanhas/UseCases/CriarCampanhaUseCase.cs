using ConexaoSolidaria.Application.Campanhas.DTOs;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Repositories;

namespace ConexaoSolidaria.Application.Campanhas.UseCases;

public sealed class CriarCampanhaUseCase
{
    private readonly ICampanhaRepository _campanhaRepository;

    public CriarCampanhaUseCase
    (
        ICampanhaRepository campanhaRepository
    )
    {
        _campanhaRepository = campanhaRepository;
    }

    public async Task<Guid> Executar(CriarCampanhaRequest request)
    {
        Campanha campanha = new(
            request.Titulo,
            request.Descricao,
            request.DataInicio,
            request.DataFim,
            request.MetaFinanceira);

        await _campanhaRepository.Adicionar(campanha);
        await _campanhaRepository.UnitOfWork.Commit();

        return campanha.Id;
    }
}
