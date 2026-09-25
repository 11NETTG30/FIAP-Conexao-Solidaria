using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.Security;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class CriarUsuarioUseCaseTests
{
    private const string NomeValido = "João Silva";
    private const string EmailValido = "joao.silva@conexaosolidaria.com.br";
    private const string SenhaValida = "SenhaForte123!";

    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<ISenhaHasher> _senhaHasherMock = new();
    private readonly CriarUsuarioUseCase _useCase;

    public CriarUsuarioUseCaseTests()
    {
        _usuarioRepositoryMock
            .Setup(r => r.UnitOfWork)
            .Returns(Mock.Of<IUnitOfWork>());

        _useCase = new CriarUsuarioUseCase(_usuarioRepositoryMock.Object, _senhaHasherMock.Object);
    }

    private static SenhaHash SenhaHashValida() =>
        new(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));

    [Fact]
    public async Task AoCriarUsuarioComEmailJaExistenteDeveLancarConflictException()
    {
        // Arrange
        _senhaHasherMock
            .Setup(h => h.GerarHash(It.IsAny<SenhaTextoPuro>()))
            .Returns(SenhaHashValida());
        _usuarioRepositoryMock
            .Setup(r => r.VerificarExistenciaEmail(It.IsAny<string>()))
            .ReturnsAsync(true);

        CriarUsuarioRequest request = new(NomeValido, EmailValido, SenhaValida, SenhaValida);

        // Act
        ConflictException ex = await Assert.ThrowsAsync<ConflictException>(() => _useCase.Executar(request));

        // Assert
        Assert.Equal("Já existe um usuário cadastrado com esse e-mail", ex.Message);
        _usuarioRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task AoCriarUsuarioComEmailInvalidoDeveLancarValidationException()
    {
        // Arrange
        CriarUsuarioRequest request = new(NomeValido, "email-invalido", SenhaValida, SenhaValida);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        _usuarioRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task AoCriarUsuarioComSenhaFracaDeveLancarValidationException()
    {
        // Arrange
        const string senhaFraca = "fraca";
        CriarUsuarioRequest request = new(NomeValido, EmailValido, senhaFraca, senhaFraca);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        _usuarioRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task AoCriarUsuarioComConfirmacaoSenhaDivergenteDeveLancarValidationException()
    {
        // Arrange
        CriarUsuarioRequest request = new(NomeValido, EmailValido, SenhaValida, "OutraSenha123!");

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        Assert.Equal("'Senha' e 'Confirmação de Senha' são diferentes", ex.Message);
    }

    [Fact]
    public async Task AoCriarUsuarioComDadosValidosDeveAdicionarUsuarioComPerfilDoadorERetornarId()
    {
        // Arrange
        _senhaHasherMock
            .Setup(h => h.GerarHash(It.IsAny<SenhaTextoPuro>()))
            .Returns(SenhaHashValida());
        _usuarioRepositoryMock
            .Setup(r => r.VerificarExistenciaEmail(It.IsAny<string>()))
            .ReturnsAsync(false);

        CriarUsuarioRequest request = new(NomeValido, EmailValido, SenhaValida, SenhaValida);

        // Act
        Guid id = await _useCase.Executar(request);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        _usuarioRepositoryMock.Verify(r => r.Adicionar(It.Is<Usuario>(u =>
            u.Nome == NomeValido &&
            u.Email.Valor == EmailValido &&
            u.Perfil == PerfilUsuario.Doador)), Times.Once);
    }
}
