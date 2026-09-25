using ConexaoSolidaria.Application.Campanhas.DTOs;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Application.Campanhas.UseCases;

public sealed class EditarCampanhaUseCase
{
    private readonly ICampanhaRepository _campanhaRepository;

    public EditarCampanhaUseCase
    (
        ICampanhaRepository campanhaRepository
    )
    {
        _campanhaRepository = campanhaRepository;
    }

    public async Task Executar(Guid id, EditarCampanhaRequest request)
    {
        Campanha campanha = await _campanhaRepository.ObterPorIdTracking(id)
            ?? throw new NotFoundException("Campanha não encontrada");

        campanha.GarantirQuePodeSerEditada();

        if (request.Titulo is not null)
            campanha.SetTitulo(request.Titulo);

        if (request.Descricao is not null)
            campanha.SetDescricao(request.Descricao);

        // Período é validado em conjunto (DataFim > DataInicio): o campo que não
        // vier no request mantém o valor atual da campanha.
        if (request.DataInicio is not null || request.DataFim is not null)
            campanha.SetPeriodo(
                request.DataInicio ?? campanha.DataInicio,
                request.DataFim ?? campanha.DataFim);

        if (request.MetaFinanceira is not null)
            campanha.SetMetaFinanceira(request.MetaFinanceira.Value);

        if (request.Status is not null)
            campanha.SetStatus(request.Status.Value);

        await _campanhaRepository.UnitOfWork.Commit();
    }
}
