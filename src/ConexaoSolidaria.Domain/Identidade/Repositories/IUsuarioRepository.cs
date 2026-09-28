using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Shared.Abstractions;

namespace ConexaoSolidaria.Domain.Identidade.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<List<Usuario>> ListarTodos();
    Task<Usuario?> ObterPorId(Guid id);
    Task<Usuario?> ObterPorIdTracking(Guid id);
    Task<Usuario?> ObterPorEmail(string email);
    Task Adicionar(Usuario usuario);
    Task<bool> VerificarExistenciaEmail(string email);
    Task<bool> VerificarExistenciaCpf(string cpf);
}