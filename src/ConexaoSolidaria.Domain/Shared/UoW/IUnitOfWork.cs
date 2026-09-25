namespace ConexaoSolidaria.Domain.Shared.UoW;

public interface IUnitOfWork
{
    Task<bool> Commit();
}