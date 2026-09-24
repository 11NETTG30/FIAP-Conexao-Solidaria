using FCG.Application.Campanhas.DTOs;
using FCG.Domain.Campanhas.Entities;
using FCG.Domain.Campanhas.Repositories;

namespace FCG.Application.Campanhas.UseCases;

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
