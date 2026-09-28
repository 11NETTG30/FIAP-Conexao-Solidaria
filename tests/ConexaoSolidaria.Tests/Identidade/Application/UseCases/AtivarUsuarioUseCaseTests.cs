using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class AtivarUsuarioUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AtivarUsuarioUseCase _useCase;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public AtivarUsuarioUseCaseTests()
    {
        _usuarioRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _useCase = new AtivarUsuarioUseCase(_usuarioRepositoryMock.Object);
    }

    private static Usuario CriarUsuarioInativo()
    {
        Usuario usuario = new(
            "João Silva",
            new Email("joao.silva@conexaosolidaria.com.br"),
            new Cpf("11144477735"),
            new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
            PerfilUsuario.Doador);

        usuario.SetAtivo(false);

        return usuario;
    }

    [Fact]
    public async Task AoAtivarUsuarioInexistenteDeveLancarValidationException()
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
    public async Task AoAtivarUsuarioDeveDefinirAtivoVerdadeiroEChamarCommit()
    {
        // Arrange
        Usuario usuario = CriarUsuarioInativo();
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(usuario);

        // Act
        await _useCase.Executar(_usuarioId);

        // Assert
        Assert.True(usuario.Ativo);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }
}
