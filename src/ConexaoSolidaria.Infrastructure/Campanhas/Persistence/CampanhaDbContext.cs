using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Infrastructure.Campanhas.Persistence.Configurations;
using ConexaoSolidaria.Infrastructure.Shared.Persistence.UoW;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.Infrastructure.Campanhas.Persistence;

public sealed class CampanhaDbContext : DbContextUoW
{
    public const string SCHEMA = "campanha";

    public DbSet<Campanha> Campanhas => Set<Campanha>();

    public CampanhaDbContext(DbContextOptions<CampanhaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SCHEMA);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CampanhaDbContext).Assembly,
            type => type.Namespace == typeof(CampanhaConfiguration).Namespace
        );
    }

}
