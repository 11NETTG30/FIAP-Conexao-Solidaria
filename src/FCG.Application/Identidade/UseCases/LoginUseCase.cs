using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.Security;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Identidade.Security;

namespace FCG.Application.Identidade.UseCases;

public sealed class LoginUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly ISenhaHasher _senhaHasher;
    private readonly ITokenSettings _tokenSettings;

    public LoginUseCase
    (
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        ISenhaHasher senhaHasher,
        ITokenSettings tokenSettings
    )
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _senhaHasher = senhaHasher;
        _tokenSettings = tokenSettings;
    }
    
    public async Task<AuthResponse> Executar(LoginRequest request)
    {
        const string mensagemErroPadrao = "E-mail ou senha inválidos";

        Usuario usuario = await _usuarioRepository.ObterPorEmail(request.Email)
            ?? throw new UnauthorizedAccessException(mensagemErroPadrao);

        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativado");
        
        bool senhaValida = _senhaHasher.ValidarSenha(request.Senha, usuario.SenhaHash);
        
        if (!senhaValida)
            throw new UnauthorizedAccessException(mensagemErroPadrao);

        string accessToken = _jwtService.GerarAccessToken(usuario);
        RefreshToken refreshToken = new(usuario.Id, _tokenSettings.ExpiracaoRefreshTokenDias);

        await _refreshTokenRepository.Adicionar(refreshToken);
        await _refreshTokenRepository.UnitOfWork.Commit();
        
        return new AuthResponse
        (
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            ExpiracaoAccessToken: DateTime.UtcNow.AddMinutes(_tokenSettings.ExpiracaoAccessTokenMinutos),
            ExpiracaoRefreshToken: refreshToken.ExpiracaoEm,
            Usuario: new UsuarioTokenDto
            (
                Id: usuario.Id,
                Nome: usuario.Nome,
                Email: usuario.Email.Valor,
                Perfil: (byte)usuario.Perfil
            )
        );
    }
    
}