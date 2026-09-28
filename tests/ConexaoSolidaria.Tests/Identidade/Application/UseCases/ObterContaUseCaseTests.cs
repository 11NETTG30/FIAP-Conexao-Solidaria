using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class ObterContaUseCaseTests
{
    private readonly Mock<IInformacoesUsuarioLogado> _informacoesUsuarioLogadoMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly ObterContaUseCase _useCase;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public ObterContaUseCaseTests()
    {
        _informacoesUsuarioLogadoMock.Setup(i => i.Id).Returns(_usuarioId);
        _useCase = new ObterContaUseCase(_informacoesUsuarioLogadoMock.Object, _usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task AoObterContaComUsuarioInexistenteDeveRetornarNull()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorId(_usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act
        UsuarioDto? resultado = await _useCase.Executar();

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task AoObterContaComUsuarioExistenteDeveRetornarUsuarioDtoCorrespondente()
    {
        // Arrange
        Usuario usuario = new(
            "João Silva",
            new Email("joao.silva@conexaosolidaria.com.br"),
            new Cpf("11144477735"),
            new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
            PerfilUsuario.Doador);

        _usuarioRepositoryMock
            .Setup(r => r.ObterPorId(_usuarioId))
            .ReturnsAsync(usuario);

        // Act
        UsuarioDto? resultado = await _useCase.Executar();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuario.Id, resultado.Id);
        Assert.Equal(usuario.Nome, resultado.Nome);
    }
}
