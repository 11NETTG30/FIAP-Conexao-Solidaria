using FCG.Domain.Campanhas.Entities;
using FCG.Domain.Campanhas.Enums;
using FCG.Domain.Campanhas.Repositories;
using FCG.Domain.Shared.UoW;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Campanhas.Persistence.Repositories;

public sealed class CampanhaRepository : ICampanhaRepository
{
    private readonly CampanhaDbContext _dbContext;
    public IUnitOfWork UnitOfWork => _dbContext;

    public CampanhaRepository(CampanhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Ativa e dentro do período: campanha agendada (DataInicio futura) só aparece
    // quando começa, e vencida (DataFim passada) sai mesmo sem o gestor ter
    // concluído — o worker de doações filtra só a DataFim
    public async Task<List<Campanha>> ListarAtivas()
    {
        DateTime agora = DateTime.UtcNow;

        return await _dbContext.Campanhas
            .AsNoTracking()
            .Where(c => c.Status == StatusCampanha.Ativa && c.DataInicio <= agora && c.DataFim >= agora)
            .OrderBy(c => c.DataFim)
            .ToListAsync();
    }

    public async Task<Campanha?> ObterPorIdTracking(Guid id)
    {
        return await _dbContext.Campanhas
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Adicionar(Campanha campanha)
    {
        await _dbContext.Campanhas
            .AddAsync(campanha);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
