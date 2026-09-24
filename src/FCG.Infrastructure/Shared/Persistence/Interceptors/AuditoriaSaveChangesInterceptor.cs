using FCG.Domain.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FCG.Infrastructure.Shared.Persistence.Interceptors;

public sealed class AuditoriaSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges
    (
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        AtualizarEntidadesAuditaveis(eventData.Context);
        
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync
    (
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        AtualizarEntidadesAuditaveis(eventData.Context);
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void AtualizarEntidadesAuditaveis(DbContext? context)
    {
        if (context is null)
            return;

        DateTime dataAtual = DateTime.UtcNow;

        IEnumerable<EntityEntry<IAuditavel>> entidadesAuditaveis = context.ChangeTracker
            .Entries<IAuditavel>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (EntityEntry<IAuditavel> entry in entidadesAuditaveis)
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Property(nameof(IAuditavel.DataCriacao)).IsModified = false;
                    entry.Property(nameof(IAuditavel.DataAtualizacao)).CurrentValue = dataAtual;
                    break;
                case EntityState.Added:
                    entry.Property(nameof(IAuditavel.DataCriacao)).CurrentValue = dataAtual;
                    entry.Property(nameof(IAuditavel.DataAtualizacao)).CurrentValue = null;
                    break;
            }
        }
    }
}