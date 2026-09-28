using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.Security;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class AlterarSenhaUseCaseTests
{
    private const string NomeValido = "João Silva";
    private const string EmailValido = "joao.silva@conexaosolidaria.com.br";

    private readonly Mock<IInformacoesUsuarioLogado> _informacoesUsuarioLogadoMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<ISenhaHasher> _senhaHasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AlterarSenhaUseCase _useCase;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public AlterarSenhaUseCaseTests()
    {
        _informacoesUsuarioLogadoMock.Setup(i => i.Id).Returns(_usuarioId);
        _usuarioRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _useCase = new AlterarSenhaUseCase(
            _informacoesUsuarioLogadoMock.Object,
            _usuarioRepositoryMock.Object,
            _senhaHasherMock.Object);
    }

    private static Usuario CriarUsuario() => new(
        NomeValido,
        new Email(EmailValido),
        new Cpf("11144477735"),
        new SenhaHash(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
        PerfilUsuario.Doador);

    [Fact]
    public async Task AoAlterarSenhaComUsuarioNaoEncontradoDeveLancarValidationException()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync((Usuario?)null);

        AlterarSenhaRequest request = new("SenhaAtual123!", "SenhaNova123!", "SenhaNova123!");

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        Assert.Equal("Usuário não encontrado", ex.Message);
    }

    [Fact]
    public async Task AoAlterarSenhaComSenhaAtualInvalidaDeveLancarValidationException()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(CriarUsuario());
        _senhaHasherMock
            .Setup(h => h.ValidarSenha(It.IsAny<string>(), It.IsAny<SenhaHash>()))
            .Returns(false);

        AlterarSenhaRequest request = new("SenhaAtualErrada1!", "SenhaNova123!", "SenhaNova123!");

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        Assert.Equal("Senha atual inválida", ex.Message);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task AoAlterarSenhaComNovaSenhaIgualASenhaAtualDeveLancarValidationException()
    {
        // Arrange
        const string senhaAtual = "SenhaAtual123!";

        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(CriarUsuario());
        _senhaHasherMock
            .Setup(h => h.ValidarSenha(It.IsAny<string>(), It.IsAny<SenhaHash>()))
            .Returns(true);

        AlterarSenhaRequest request = new(senhaAtual, senhaAtual, senhaAtual);

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        Assert.Equal("Nova senha deve ser diferente da senha atual", ex.Message);
    }

    [Fact]
    public async Task AoAlterarSenhaComNovaSenhaFracaDeveLancarValidationException()
    {
        // Arrange
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(CriarUsuario());
        _senhaHasherMock
            .Setup(h => h.ValidarSenha(It.IsAny<string>(), It.IsAny<SenhaHash>()))
            .Returns(true);

        AlterarSenhaRequest request = new("SenhaAtual123!", "fraca", "fraca");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
    }

    [Fact]
    public async Task AoAlterarSenhaComDadosValidosDeveAtualizarSenhaHashEChamarCommit()
    {
        // Arrange
        Usuario usuario = CriarUsuario();
        SenhaHash novaSenhaHash = new(new string('b', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));

        _usuarioRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_usuarioId))
            .ReturnsAsync(usuario);
        _senhaHasherMock
            .Setup(h => h.ValidarSenha(It.IsAny<string>(), It.IsAny<SenhaHash>()))
            .Returns(true);
        _senhaHasherMock
            .Setup(h => h.GerarHash(It.IsAny<SenhaTextoPuro>()))
            .Returns(novaSenhaHash);

        AlterarSenhaRequest request = new("SenhaAtual123!", "SenhaNova123!", "SenhaNova123!");

        // Act
        await _useCase.Executar(request);

        // Assert
        Assert.Equal(novaSenhaHash, usuario.SenhaHash);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }
}
