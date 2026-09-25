using ConexaoSolidaria.Application.Campanhas.DTOs;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Repositories;

namespace ConexaoSolidaria.Application.Campanhas.UseCases;

public sealed class ListarCampanhasAtivasUseCase
{
    private readonly ICampanhaRepository _campanhaRepository;

    public ListarCampanhasAtivasUseCase
    (
        ICampanhaRepository campanhaRepository
    )
    {
        _campanhaRepository = campanhaRepository;
    }

    public async Task<IEnumerable<CampanhaAtivaDto>> Executar()
    {
        List<Campanha> campanhas = await _campanhaRepository.ListarAtivas();

        return campanhas.Select(campanha => (CampanhaAtivaDto)campanha);
    }
}
