using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class TornarUsuarioAdministradorUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly TornarUsuarioAdministradorUseCase _useCase;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public TornarUsuarioAdministradorUseCaseTests()
    {
        _usuarioRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _useCase = new TornarUsuarioAdministradorUseCase(_usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task AoTornarAdministradorUsuarioInexistenteDeveLancarValidationException()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(_usuarioId));
        Assert.Equal("Usuário não encontrado", ex.Message);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task AoTornarAdministradorDeveDefinirPerfilGestorONGEChamarCommit()
    {
        // Arrange
        Usuario usuario = new(
            "João Silva",
            new Email("joao.silva@conexaosolidaria.com.br"),
            new Cpf("11144477735"),
            new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
            PerfilUsuario.Doador);

        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(usuario);

        // Act
        await _useCase.Executar(_usuarioId);

        // Assert
        Assert.Equal(PerfilUsuario.GestorONG, usuario.Perfil);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }
}
