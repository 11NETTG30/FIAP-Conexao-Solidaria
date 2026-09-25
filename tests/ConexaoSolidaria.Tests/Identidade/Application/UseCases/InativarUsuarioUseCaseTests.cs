using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class InativarUsuarioUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly InativarUsuarioUseCase _useCase;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public InativarUsuarioUseCaseTests()
    {
        _usuarioRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _useCase = new InativarUsuarioUseCase(_usuarioRepositoryMock.Object, _refreshTokenRepositoryMock.Object);
    }

    private static Usuario CriarUsuario() => new(
        "João Silva",
        new Email("joao.silva@conexaosolidaria.com.br"),
        new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
        PerfilUsuario.Doador);

    [Fact]
    public async Task AoInativarUsuarioInexistenteDeveLancarValidationException()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(_usuarioId));
        Assert.Equal("Usuário não existe", ex.Message);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task AoInativarUsuarioDeveDefinirAtivoFalso()
    {
        // Arrange
        Usuario usuario = CriarUsuario();
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(usuario);
        _refreshTokenRepositoryMock
            .Setup(r => r.ListarNaoRevogadosPorUsuario(usuario.Id))
            .ReturnsAsync([]);

        // Act
        await _useCase.Executar(_usuarioId);

        // Assert
        Assert.False(usuario.Ativo);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task AoInativarUsuarioDeveRevogarTodosOsRefreshTokensNaoRevogados()
    {
        // Arrange
        Usuario usuario = CriarUsuario();
        RefreshToken token1 = new(usuario.Id, 7);
        RefreshToken token2 = new(usuario.Id, 7);

        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(usuario);
        _refreshTokenRepositoryMock
            .Setup(r => r.ListarNaoRevogadosPorUsuario(usuario.Id))
            .ReturnsAsync([token1, token2]);

        // Act
        await _useCase.Executar(_usuarioId);

        // Assert
        Assert.True(token1.Revogado);
        Assert.True(token2.Revogado);
        Assert.Equal(MotivoRevogacaoRefreshToken.InativacaoUsuario, token1.MotivoRevogacao);
        Assert.Equal(MotivoRevogacaoRefreshToken.InativacaoUsuario, token2.MotivoRevogacao);
    }

    [Fact]
    public async Task AoInativarUsuarioSemRefreshTokensNaoDeveLancarExcecao()
    {
        // Arrange
        Usuario usuario = CriarUsuario();
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(usuario);
        _refreshTokenRepositoryMock
            .Setup(r => r.ListarNaoRevogadosPorUsuario(usuario.Id))
            .ReturnsAsync([]);

        // Act
        Exception? ex = await Record.ExceptionAsync(() => _useCase.Executar(_usuarioId));

        // Assert
        Assert.Null(ex);
    }
}
