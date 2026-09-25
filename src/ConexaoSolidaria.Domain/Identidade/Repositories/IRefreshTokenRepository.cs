using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Shared.Abstractions;

namespace ConexaoSolidaria.Domain.Identidade.Repositories;

public interface IRefreshTokenRepository: IRepository<RefreshToken>
{
    Task<RefreshToken?> ObterPorId(Guid id);
    Task<RefreshToken?> ObterPorToken(Guid refreshToken);
    Task Adicionar(RefreshToken refreshToken);
    Task<List<RefreshToken>> ListarNaoRevogadosPorUsuario(Guid usuarioId);
}
