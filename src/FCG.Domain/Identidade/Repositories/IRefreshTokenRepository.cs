using FCG.Domain.Identidade.Entities;
using FCG.Domain.Shared.Abstractions;

namespace FCG.Domain.Identidade.Repositories;

public interface IRefreshTokenRepository: IRepository<RefreshToken>
{
    Task<RefreshToken?> ObterPorId(Guid id);
    Task<RefreshToken?> ObterPorToken(Guid refreshToken);
    Task Adicionar(RefreshToken refreshToken);
    Task<List<RefreshToken>> ListarNaoRevogadosPorUsuario(Guid usuarioId);
}
