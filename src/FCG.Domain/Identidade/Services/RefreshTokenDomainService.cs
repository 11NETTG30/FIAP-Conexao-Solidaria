using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Shared.Abstractions;

namespace FCG.Domain.Identidade.Services;

public sealed class RefreshTokenDomainService : IRefreshTokenDomainService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IDomainLogger<RefreshTokenDomainService> _logger;

    public RefreshTokenDomainService
    (
        IRefreshTokenRepository refreshTokenRepository,
        IDomainLogger<RefreshTokenDomainService> logger
    )
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task RevogarCadeiaDescendente(RefreshToken tokenInicial, Guid idTokenComprometido)
    {
        if (tokenInicial.SubstituidoPorId is null)
            return;
        
        RefreshToken? tokenFilho = await _refreshTokenRepository.ObterPorId(tokenInicial.SubstituidoPorId.Value);

        if (tokenFilho is null)
            return;

        if (!tokenFilho.Revogado)
        {
            tokenFilho.Revogar(MotivoRevogacaoRefreshToken.TokenAscendenteComprometido);
            
            _logger.LogWarning(
                "AVISO DE SEGURANÇA! RefreshToken com Id:{idToken} foi revogado pois RefreshToken ascendente de Id:{IdTokenComprometido} foi comprometido",
                tokenFilho.Id.ToString(),
                idTokenComprometido.ToString());
        }

        // Recursivamente revoga descendentes
        await RevogarCadeiaDescendente(tokenFilho, idTokenComprometido);
    }
}