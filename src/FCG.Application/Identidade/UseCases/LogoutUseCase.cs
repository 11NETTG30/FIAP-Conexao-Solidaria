using FCG.Application.Identidade.DTOs;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.Repositories;

namespace FCG.Application.Identidade.UseCases;

public sealed class LogoutUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutUseCase
    (
        IRefreshTokenRepository refreshTokenRepository
    )
    {
        _refreshTokenRepository = refreshTokenRepository;
    }
    
    public async Task Executar(LogoutRequest request)
    {
        RefreshToken? token = await _refreshTokenRepository.ObterPorToken(request.RefreshToken);

        if (token is null)
            return;

        token.Revogar(MotivoRevogacaoRefreshToken.Logout);

        await _refreshTokenRepository.UnitOfWork.Commit();
    }
}