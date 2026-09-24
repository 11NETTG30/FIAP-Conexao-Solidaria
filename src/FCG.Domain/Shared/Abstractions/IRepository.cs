using FCG.Domain.Shared.UoW;

namespace FCG.Domain.Shared.Abstractions;

public interface IRepository<T> : IDisposable where T : Entity, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}