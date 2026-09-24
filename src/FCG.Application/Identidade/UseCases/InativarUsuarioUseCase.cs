using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Application.Identidade.UseCases;

public sealed class InativarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    
    public InativarUsuarioUseCase
    (
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository
    )
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task Executar(Guid usuarioId)
    {
        Usuario usuario = await _usuarioRepository.ObterPorIdTracking(usuarioId)
            ?? throw new ValidationException("Usuário não existe");
        
        usuario.SetAtivo(false);

        List<RefreshToken> refreshTokens = await _refreshTokenRepository
            .ListarNaoRevogadosPorUsuario(usuario.Id);
        
        refreshTokens.ForEach(refreshToken =>
        {
            refreshToken.Revogar(MotivoRevogacaoRefreshToken.InativacaoUsuario);
        });
        
        await _usuarioRepository.UnitOfWork.Commit();
    }
}