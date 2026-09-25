using ConexaoSolidaria.Domain.Shared.UoW;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.Infrastructure.Shared.Persistence.UoW;

public abstract class DbContextUoW: DbContext, IUnitOfWork
{
    protected DbContextUoW(DbContextOptions options) : base(options)
    {
        
    }
    
    public async Task<bool> Commit()
    {
        return await SaveChangesAsync() > 0;
    }
}