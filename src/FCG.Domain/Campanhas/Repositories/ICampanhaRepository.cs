using FCG.Domain.Campanhas.Entities;
using FCG.Domain.Shared.Abstractions;

namespace FCG.Domain.Campanhas.Repositories;

public interface ICampanhaRepository : IRepository<Campanha>
{
    Task<List<Campanha>> ListarAtivas();
    Task<Campanha?> ObterPorIdTracking(Guid id);
    Task Adicionar(Campanha campanha);
}
