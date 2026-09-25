using ConexaoSolidaria.Domain.Shared.UoW;

namespace ConexaoSolidaria.Domain.Shared.Abstractions;

public interface IRepository<T> : IDisposable where T : Entity, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}