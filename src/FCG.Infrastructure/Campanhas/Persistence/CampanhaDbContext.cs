using FCG.Domain.Campanhas.Entities;
using FCG.Infrastructure.Campanhas.Persistence.Configurations;
using FCG.Infrastructure.Shared.Persistence.UoW;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Campanhas.Persistence;

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
