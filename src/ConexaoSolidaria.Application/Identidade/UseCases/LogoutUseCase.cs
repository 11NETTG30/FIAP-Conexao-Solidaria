using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;

namespace ConexaoSolidaria.Application.Identidade.UseCases;

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