using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.Security;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.Security;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class LoginUseCaseTests
{
    private const string EmailValido = "joao.silva@conexaosolidaria.com.br";
    private const string SenhaValida = "SenhaForte123!";

    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<ISenhaHasher> _senhaHasherMock = new();
    private readonly Mock<ITokenSettings> _tokenSettingsMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _refreshTokenRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _tokenSettingsMock.Setup(t => t.ExpiracaoRefreshTokenDias).Returns((byte)7);
        _tokenSettingsMock.Setup(t => t.ExpiracaoAccessTokenMinutos).Returns((short)15);

        _useCase = new LoginUseCase(
            _usuarioRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _jwtServiceMock.Object,
            _senhaHasherMock.Object,
            _tokenSettingsMock.Object);
    }

    private static Usuario CriarUsuarioAtivo() => new(
        "João Silva",
        new Email(EmailValido),
        new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
        PerfilUsuario.Doador);

    [Fact]
    public async Task AoLogarComEmailInexistenteDeveLancarUnauthorizedAccessException()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorEmail(EmailValido))
            .ReturnsAsync((Usuario?)null);

        LoginRequest request = new(EmailValido, SenhaValida);

        // Act & Assert
        UnauthorizedAccessException ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        Assert.Equal("E-mail ou senha inválidos", ex.Message);
    }

    [Fact]
    public async Task AoLogarComUsuarioInativoDeveLancarUnauthorizedAccessException()
    {
        // Arrange
        Usuario usuario = CriarUsuarioAtivo();
        usuario.SetAtivo(false);

        _usuarioRepositoryMock
            .Setup(r => r.ObterPorEmail(EmailValido))
            .ReturnsAsync(usuario);

        LoginRequest request = new(EmailValido, SenhaValida);

        // Act & Assert
        UnauthorizedAccessException ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        Assert.Equal("Usuário inativado", ex.Message);
    }

    [Fact]
    public async Task AoLogarComSenhaInvalidaDeveLancarUnauthorizedAccessException()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorEmail(EmailValido))
            .ReturnsAsync(CriarUsuarioAtivo());
        _senhaHasherMock
            .Setup(h => h.ValidarSenha(It.IsAny<string>(), It.IsAny<SenhaHash>()))
            .Returns(false);

        LoginRequest request = new(EmailValido, "SenhaErrada123!");

        // Act & Assert
        UnauthorizedAccessException ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Executar(request));
        Assert.Equal("E-mail ou senha inválidos", ex.Message);
        _refreshTokenRepositoryMock.Verify(r => r.Adicionar(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task AoLogarComCredenciaisValidasDeveRetornarAuthResponseEAdicionarRefreshToken()
    {
        // Arrange
        Usuario usuario = CriarUsuarioAtivo();

        _usuarioRepositoryMock
            .Setup(r => r.ObterPorEmail(EmailValido))
            .ReturnsAsync(usuario);
        _senhaHasherMock
            .Setup(h => h.ValidarSenha(It.IsAny<string>(), It.IsAny<SenhaHash>()))
            .Returns(true);
        _jwtServiceMock
            .Setup(j => j.GerarAccessToken(usuario))
            .Returns("access-token-fake");

        LoginRequest request = new(EmailValido, SenhaValida);

        // Act
        AuthResponse resposta = await _useCase.Executar(request);

        // Assert
        Assert.Equal("access-token-fake", resposta.AccessToken);
        Assert.Equal(usuario.Id, resposta.Usuario.Id);
        Assert.Equal(usuario.Email.Valor, resposta.Usuario.Email);
        _refreshTokenRepositoryMock.Verify(r => r.Adicionar(It.Is<RefreshToken>(t => t.UsuarioId == usuario.Id)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }
}
