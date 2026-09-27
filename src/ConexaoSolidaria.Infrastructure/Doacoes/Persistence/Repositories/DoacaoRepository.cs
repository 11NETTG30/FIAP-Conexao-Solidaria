using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using ConexaoSolidaria.Domain.Shared.UoW;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.Infrastructure.Doacoes.Persistence.Repositories;

public sealed class DoacaoRepository : IDoacaoRepository
{
    private readonly DoacaoDbContext _dbContext;
    public IUnitOfWork UnitOfWork => _dbContext;

    public DoacaoRepository(DoacaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Adicionar(Doacao doacao)
    {
        await _dbContext.Doacoes
            .AddAsync(doacao);
    }

    public async Task<Doacao?> ObterPorId(Guid id)
    {
        return await _dbContext.Doacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}