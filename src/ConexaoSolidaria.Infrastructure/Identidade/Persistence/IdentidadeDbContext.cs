using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Infrastructure.Identidade.Persistence.Configurations;
using ConexaoSolidaria.Infrastructure.Shared.Persistence.UoW;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.Infrastructure.Identidade.Persistence;

public sealed class IdentidadeDbContext : DbContextUoW
{
    public const string SCHEMA = "identidade";
    
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    public IdentidadeDbContext(DbContextOptions<IdentidadeDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SCHEMA);
        
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentidadeDbContext).Assembly, 
            type => type.Namespace == typeof(UsuarioConfiguration).Namespace
        );
    }
    
}