using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Shared.Abstractions;

namespace ConexaoSolidaria.Domain.Doacoes.Repositories;

public interface IDoacaoRepository : IRepository<Doacao>
{
    Task Adicionar(Doacao doacao);
    Task<Doacao?> ObterPorId(Guid id);
    Task<ResultadoPaginado<Doacao>> Listar(FiltroListagemDoacao filtro);
}