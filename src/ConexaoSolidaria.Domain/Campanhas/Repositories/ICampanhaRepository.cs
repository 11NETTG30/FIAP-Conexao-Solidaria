using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Shared.Abstractions;

namespace ConexaoSolidaria.Domain.Campanhas.Repositories;

public interface ICampanhaRepository : IRepository<Campanha>
{
    Task<List<Campanha>> ListarAtivas();
    Task<Campanha?> ObterPorId(Guid id);
    Task<Campanha?> ObterPorIdTracking(Guid id);
    Task Adicionar(Campanha campanha);
}
