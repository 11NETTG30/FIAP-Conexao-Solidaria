using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.Security;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Identidade.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Application.Identidade.UseCases;

public sealed class RefreshTokenUseCase
{
    private readonly ILogger<RefreshTokenUseCase> _logger;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly ITokenSettings _tokenSettings;
    private readonly IRefreshTokenDomainService _refreshTokenDomainService;
    
    public RefreshTokenUseCase
    (
        ILogger<RefreshTokenUseCase> logger,
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        ITokenSettings tokenSettings,
        IRefreshTokenDomainService  refreshTokenDomainService
    )
    {
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _tokenSettings = tokenSettings;
        _refreshTokenDomainService  = refreshTokenDomainService;
    }
    
    public async Task<AuthResponse> Executar(RefreshRequest request)
    {
        const string mensagemErroPadrao = "Refresh token inválido ou expirado";
        
        RefreshToken? refreshToken = await _refreshTokenRepository.ObterPorToken(request.RefreshToken);

        if (refreshToken is null)
            throw new UnauthorizedAccessException(mensagemErroPadrao);

        if (refreshToken.Revogado)
            await ExecutarRotinaDeSegurancaPorReusoRefreshToken(refreshToken);
        
        if (!refreshToken.ValidarTokenValido())
            throw new UnauthorizedAccessException(mensagemErroPadrao);
        
        Usuario? usuario = await _usuarioRepository.ObterPorId(refreshToken.UsuarioId)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado");

        RefreshToken novoRefreshToken = new(usuario.Id, _tokenSettings.ExpiracaoRefreshTokenDias);
        
        refreshToken.Substituir(novoRefreshToken);

        await _refreshTokenRepository.Adicionar(novoRefreshToken);

        string accessToken = _jwtService.GerarAccessToken(usuario);
        
        await _refreshTokenRepository.UnitOfWork.Commit();
        
        return new AuthResponse
        (
            AccessToken: accessToken,
            RefreshToken: novoRefreshToken.Token,
            ExpiracaoAccessToken: DateTime.UtcNow.AddMinutes(_tokenSettings.ExpiracaoAccessTokenMinutos),
            ExpiracaoRefreshToken: novoRefreshToken.ExpiracaoEm,
            Usuario: new UsuarioTokenDto
            (
                Id: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email.Valor,
                Perfil: (byte)usuario.Perfil
            )
        );
    }

    private async Task ExecutarRotinaDeSegurancaPorReusoRefreshToken(RefreshToken refreshToken)
    {
        if (!_tokenSettings.HabilitarSegurancaDeReusoRefreshToken)
            return;
        
        _logger.LogWarning(
            "AVISO DE SEGURANÇA! Reuso de RefreshToken detectado. UsuarioId: {UsuarioId}, TokenId: {TokenId}",
            refreshToken.UsuarioId,
            refreshToken.Id);
            
        await _refreshTokenDomainService.RevogarCadeiaDescendente(refreshToken, refreshToken.Id);
        await _refreshTokenRepository.UnitOfWork.Commit();
            
        throw new UnauthorizedAccessException(
            "Atividade suspeita detectada. Faça login novamente.");
    }
    
}