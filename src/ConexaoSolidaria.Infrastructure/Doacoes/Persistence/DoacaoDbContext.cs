using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Infrastructure.Doacoes.Persistence.Configurations;
using ConexaoSolidaria.Infrastructure.Shared.Persistence.UoW;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.Infrastructure.Doacoes.Persistence;

public sealed class DoacaoDbContext : DbContextUoW
{
    public const string SCHEMA = "doacao";

    public DbSet<Doacao> Doacoes => Set<Doacao>();
    public DbSet<DoacaoProcessada> DoacoesProcessadas => Set<DoacaoProcessada>();

    public DoacaoDbContext(DbContextOptions<DoacaoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SCHEMA);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DoacaoDbContext).Assembly,
            type => type.Namespace == typeof(DoacaoConfiguration).Namespace
        );

        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
