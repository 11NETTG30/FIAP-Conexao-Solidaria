using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class ObterUsuarioPorIdUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly ObterUsuarioPorIdUseCase _useCase;

    public ObterUsuarioPorIdUseCaseTests()
    {
        _useCase = new ObterUsuarioPorIdUseCase(_usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task AoObterUsuarioPorIdInexistenteDeveRetornarNull()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorId(id)).ReturnsAsync((Usuario?)null);

        // Act
        UsuarioDto? resultado = await _useCase.Executar(id);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task AoObterUsuarioPorIdExistenteDeveRetornarUsuarioDtoCorrespondente()
    {
        // Arrange
        Usuario usuario = new(
            "João Silva",
            new Email("joao.silva@conexaosolidaria.com.br"),
            new Cpf("11144477735"),
            new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
            PerfilUsuario.Doador);

        _usuarioRepositoryMock.Setup(r => r.ObterPorId(usuario.Id)).ReturnsAsync(usuario);

        // Act
        UsuarioDto? resultado = await _useCase.Executar(usuario.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuario.Id, resultado.Id);
        Assert.Equal(usuario.Email.Valor, resultado.Email);
    }
}
