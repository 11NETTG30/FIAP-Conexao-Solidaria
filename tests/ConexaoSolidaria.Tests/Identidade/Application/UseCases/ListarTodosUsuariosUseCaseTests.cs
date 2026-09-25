using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class ListarTodosUsuariosUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly ListarTodosUsuariosUseCase _useCase;

    public ListarTodosUsuariosUseCaseTests()
    {
        _useCase = new ListarTodosUsuariosUseCase(_usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task AoListarTodosUsuariosSemUsuariosCadastradosDeveRetornarListaVazia()
    {
        // Arrange
        _usuarioRepositoryMock.Setup(r => r.ListarTodos()).ReturnsAsync([]);

        // Act
        IEnumerable<UsuarioDto> resultado = await _useCase.Executar();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task AoListarTodosUsuariosDeveMapearCadaUsuarioParaUsuarioDto()
    {
        // Arrange
        Usuario usuario = new(
            "João Silva",
            new Email("joao.silva@conexaosolidaria.com.br"),
            new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
            PerfilUsuario.Doador);

        _usuarioRepositoryMock.Setup(r => r.ListarTodos()).ReturnsAsync([usuario]);

        // Act
        IEnumerable<UsuarioDto> resultado = await _useCase.Executar();

        // Assert
        UsuarioDto dto = Assert.Single(resultado);
        Assert.Equal(usuario.Id, dto.Id);
        Assert.Equal(usuario.Nome, dto.Nome);
        Assert.Equal(usuario.Email.Valor, dto.Email);
        Assert.Equal(usuario.Perfil.ToString(), dto.Perfil);
        Assert.Equal(usuario.Ativo, dto.Ativo);
    }
}
