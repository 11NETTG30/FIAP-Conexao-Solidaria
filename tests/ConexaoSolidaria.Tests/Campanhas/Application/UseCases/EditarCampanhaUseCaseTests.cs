using ConexaoSolidaria.Application.Campanhas.DTOs;
using ConexaoSolidaria.Application.Campanhas.UseCases;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Enums;
using ConexaoSolidaria.Domain.Campanhas.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Campanhas.Application.UseCases;

public class EditarCampanhaUseCaseTests
{
    private const string TituloValido = "Natal Solidário";
    private const string DescricaoValida = "Arrecadação de brinquedos para as crianças acolhidas";
    private const decimal MetaValida = 5000m;

    private static readonly DateTime DataInicioValida = DateTime.UtcNow.AddDays(-1);
    private static readonly DateTime DataFimValida = DateTime.UtcNow.AddDays(30);

    private readonly Mock<ICampanhaRepository> _campanhaRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly EditarCampanhaUseCase _useCase;
    private readonly Guid _campanhaId = Guid.NewGuid();

    public EditarCampanhaUseCaseTests()
    {
        _campanhaRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _useCase = new EditarCampanhaUseCase(_campanhaRepositoryMock.Object);
    }

    private static Campanha CriarCampanhaValida() =>
        new(TituloValido, DescricaoValida, DataInicioValida, DataFimValida, MetaValida);

    private static readonly EditarCampanhaRequest RequestVazio = new(null, null, null, null, null, null);

    [Fact]
    public async Task AoEditarCampanhaInexistenteDeveLancarNotFoundException()
    {
        // Arrange
        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync((Campanha?)null);

        // Act & Assert
        NotFoundException ex = await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Executar(_campanhaId, RequestVazio));
        Assert.Equal("Campanha não encontrada", ex.Message);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Theory]
    [InlineData(StatusCampanha.Concluida)]
    [InlineData(StatusCampanha.Cancelada)]
    public async Task AoEditarCampanhaEncerradaDeveLancarValidationException(StatusCampanha statusFinal)
    {
        // Arrange
        Campanha campanha = CriarCampanhaValida();
        campanha.SetStatus(statusFinal);

        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync(campanha);

        EditarCampanhaRequest request = RequestVazio with { Titulo = "Novo título" };

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(_campanhaId, request));
        Assert.Equal("Campanha concluída ou cancelada não pode ser editada", ex.Message);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task AoEditarApenasTituloDeveAtualizarSomenteOTitulo()
    {
        // Arrange
        Campanha campanha = CriarCampanhaValida();
        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync(campanha);

        const string novoTitulo = "Páscoa Solidária";
        EditarCampanhaRequest request = RequestVazio with { Titulo = novoTitulo };

        // Act
        await _useCase.Executar(_campanhaId, request);

        // Assert
        Assert.Equal(novoTitulo, campanha.Titulo);
        Assert.Equal(DescricaoValida, campanha.Descricao);
        Assert.Equal(MetaValida, campanha.MetaFinanceira);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task AoEditarSemAlteracoesDeveApenasChamarCommit()
    {
        // Arrange
        Campanha campanha = CriarCampanhaValida();
        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync(campanha);

        // Act
        await _useCase.Executar(_campanhaId, RequestVazio);

        // Assert
        Assert.Equal(TituloValido, campanha.Titulo);
        Assert.Equal(DataFimValida, campanha.DataFim);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task AoEditarApenasDataFimDeveManterDataInicioAtual()
    {
        // Arrange
        Campanha campanha = CriarCampanhaValida();
        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync(campanha);

        DateTime novaDataFim = DateTime.UtcNow.AddDays(60);
        EditarCampanhaRequest request = RequestVazio with { DataFim = novaDataFim };

        // Act
        await _useCase.Executar(_campanhaId, request);

        // Assert
        Assert.Equal(DataInicioValida, campanha.DataInicio);
        Assert.Equal(novaDataFim, campanha.DataFim);
    }

    [Fact]
    public async Task AoEditarComNovaDataFimNoPassadoDeveLancarValidationExceptionEManterDadosOriginais()
    {
        // Arrange
        Campanha campanha = CriarCampanhaValida();
        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync(campanha);

        EditarCampanhaRequest request = RequestVazio with { DataFim = DateTime.UtcNow.AddDays(-5) };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(_campanhaId, request));
        Assert.Equal(DataFimValida, campanha.DataFim);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task AoEditarComStatusInvalidoDeveLancarValidationException()
    {
        // Arrange
        Campanha campanha = CriarCampanhaValida();
        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync(campanha);

        EditarCampanhaRequest request = RequestVazio with { Status = (StatusCampanha)99 };

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(_campanhaId, request));
        Assert.Equal("Status da campanha é inválido, valor não definido", ex.Message);
    }

    [Fact]
    public async Task AoEditarComMetaFinanceiraZeroDeveLancarValidationException()
    {
        // Arrange
        Campanha campanha = CriarCampanhaValida();
        _campanhaRepositoryMock
            .Setup(r => r.ObterPorIdTracking(_campanhaId))
            .ReturnsAsync(campanha);

        EditarCampanhaRequest request = RequestVazio with { MetaFinanceira = 0m };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(_campanhaId, request));
        Assert.Equal(MetaValida, campanha.MetaFinanceira);
    }
}
