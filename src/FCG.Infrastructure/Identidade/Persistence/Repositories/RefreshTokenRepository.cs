using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Shared.UoW;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Identidade.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentidadeDbContext _dbContext;
    public IUnitOfWork UnitOfWork => _dbContext;

    public RefreshTokenRepository(IdentidadeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<RefreshToken?> ObterPorId(Guid id)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == id);
    }

    public async Task<RefreshToken?> ObterPorToken(Guid refreshToken)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
    }
    
    public async Task Adicionar(RefreshToken refreshToken)
    {
        await _dbContext.RefreshTokens
            .AddAsync(refreshToken);
    }

    public async Task<List<RefreshToken>> ListarNaoRevogadosPorUsuario(Guid usuarioId)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => rt.UsuarioId == usuarioId && !rt.Revogado)
            .ToListAsync();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}