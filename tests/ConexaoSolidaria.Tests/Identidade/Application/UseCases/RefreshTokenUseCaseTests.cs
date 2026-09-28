using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.Security;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.Security;
using ConexaoSolidaria.Domain.Identidade.Services;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.UoW;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class RefreshTokenUseCaseTests
{
    private readonly Mock<ILogger<RefreshTokenUseCase>> _loggerMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<ITokenSettings> _tokenSettingsMock = new();
    private readonly Mock<IRefreshTokenDomainService> _refreshTokenDomainServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RefreshTokenUseCase _useCase;

    public RefreshTokenUseCaseTests()
    {
        _refreshTokenRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _tokenSettingsMock.Setup(t => t.ExpiracaoRefreshTokenDias).Returns((byte)7);
        _tokenSettingsMock.Setup(t => t.ExpiracaoAccessTokenMinutos).Returns((short)15);

        _useCase = new RefreshTokenUseCase(
            _loggerMock.Object,
            _usuarioRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _jwtServiceMock.Object,
            _tokenSettingsMock.Object,
            _refreshTokenDomainServiceMock.Object);
    }

    private static Usuario CriarUsuario() => new(
        "João Silva",
        new Email("joao.silva@conexaosolidaria.com.br"),
        new Cpf("11144477735"),
        new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
        PerfilUsuario.Doador);

    [Fact]
    public async Task AoRenovarComTokenInexistenteDeveLancarUnauthorizedAccessException()
    {
        // Arrange
        Guid tokenInexistente = Guid.NewGuid();
        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(tokenInexistente))
            .ReturnsAsync((RefreshToken?)null);

        RefreshRequest request = new(tokenInexistente);

        // Act & Assert
        UnauthorizedAccessException ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        Assert.Equal("Refresh token inválido ou expirado", ex.Message);
    }

    [Fact]
    public async Task AoRenovarComTokenExpiradoDeveLancarUnauthorizedAccessException()
    {
        // Arrange
        RefreshToken tokenExpirado = new(Guid.NewGuid(), 0);
        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(tokenExpirado.Token))
            .ReturnsAsync(tokenExpirado);

        RefreshRequest request = new(tokenExpirado.Token);

        // Act & Assert
        UnauthorizedAccessException ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        Assert.Equal("Refresh token inválido ou expirado", ex.Message);
        _usuarioRepositoryMock.Verify(r => r.ObterPorId(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AoRenovarComTokenRevogadoESegurancaDeReusoDesabilitadaDeveLancarUnauthorizedAccessExceptionSemRevogarCadeia()
    {
        // Arrange
        RefreshToken tokenRevogado = new(Guid.NewGuid(), 7);
        tokenRevogado.Revogar(MotivoRevogacaoRefreshToken.Logout);

        _tokenSettingsMock.Setup(t => t.HabilitarSegurancaDeReusoRefreshToken).Returns(false);
        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(tokenRevogado.Token))
            .ReturnsAsync(tokenRevogado);

        RefreshRequest request = new(tokenRevogado.Token);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        _refreshTokenDomainServiceMock.Verify(
            s => s.RevogarCadeiaDescendente(It.IsAny<RefreshToken>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AoRenovarComTokenRevogadoESegurancaDeReusoHabilitadaDeveRevogarCadeiaELancarUnauthorizedAccessException()
    {
        // Arrange
        RefreshToken tokenRevogado = new(Guid.NewGuid(), 7);
        tokenRevogado.Revogar(MotivoRevogacaoRefreshToken.Logout);

        _tokenSettingsMock.Setup(t => t.HabilitarSegurancaDeReusoRefreshToken).Returns(true);
        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(tokenRevogado.Token))
            .ReturnsAsync(tokenRevogado);

        RefreshRequest request = new(tokenRevogado.Token);

        // Act & Assert
        UnauthorizedAccessException ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        Assert.Equal("Atividade suspeita detectada. Faça login novamente.", ex.Message);
        _refreshTokenDomainServiceMock.Verify(
            s => s.RevogarCadeiaDescendente(tokenRevogado, tokenRevogado.Id), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task AoRenovarComUsuarioNaoEncontradoDeveLancarUnauthorizedAccessException()
    {
        // Arrange
        Guid usuarioId = Guid.NewGuid();
        RefreshToken token = new(usuarioId, 7);

        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(token.Token))
            .ReturnsAsync(token);
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorId(usuarioId))
            .ReturnsAsync((Usuario?)null);

        RefreshRequest request = new(token.Token);

        // Act & Assert
        UnauthorizedAccessException ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        Assert.Equal("Usuário não encontrado", ex.Message);
    }

    [Fact]
    public async Task AoRenovarComTokenValidoDeveRevogarTokenAntigoEAdicionarNovoRefreshToken()
    {
        // Arrange
        Usuario usuario = CriarUsuario();
        RefreshToken tokenAtual = new(usuario.Id, 7);

        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(tokenAtual.Token))
            .ReturnsAsync(tokenAtual);
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorId(usuario.Id))
            .ReturnsAsync(usuario);
        _jwtServiceMock
            .Setup(j => j.GerarAccessToken(usuario))
            .Returns("novo-access-token");

        RefreshRequest request = new(tokenAtual.Token);

        // Act
        AuthResponse resposta = await _useCase.Executar(request);

        // Assert
        Assert.True(tokenAtual.Revogado);
        Assert.Equal(MotivoRevogacaoRefreshToken.Substituicao, tokenAtual.MotivoRevogacao);
        Assert.Equal("novo-access-token", resposta.AccessToken);
        Assert.NotEqual(tokenAtual.Token, resposta.RefreshToken);
        _refreshTokenRepositoryMock.Verify(
            r => r.Adicionar(It.Is<RefreshToken>(t => t.Token == resposta.RefreshToken)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _refreshTokenDomainServiceMock.Verify(
            s => s.RevogarCadeiaDescendente(It.IsAny<RefreshToken>(), It.IsAny<Guid>()), Times.Never);
    }
}
