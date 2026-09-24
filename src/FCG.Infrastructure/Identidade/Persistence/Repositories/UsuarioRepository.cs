using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Shared.UoW;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Identidade.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IdentidadeDbContext _dbContext;
    public IUnitOfWork UnitOfWork => _dbContext;

    public UsuarioRepository(IdentidadeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Usuario>> ListarTodos()
    {
        return await _dbContext.Usuarios
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<Usuario?> ObterPorId(Guid id)
    {
        return await _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<Usuario?> ObterPorIdTracking(Guid id)
    {
        return await _dbContext.Usuarios
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObterPorEmail(string email)
    {
        string emailNormalizado = email.Trim().ToLowerInvariant();
        
        return await _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Valor == emailNormalizado);
    }

    public async Task Adicionar(Usuario usuario)
    {
        await _dbContext.Usuarios
            .AddAsync(usuario);
    }

    public async Task<bool> VerificarExistenciaEmail(string email)
    {
        return await _dbContext.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Email.Valor == email);
    }
    
    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}