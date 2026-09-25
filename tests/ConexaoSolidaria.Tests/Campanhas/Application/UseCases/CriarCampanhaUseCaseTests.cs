using ConexaoSolidaria.Application.Campanhas.DTOs;
using ConexaoSolidaria.Application.Campanhas.UseCases;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Campanhas.Application.UseCases;

public class CriarCampanhaUseCaseTests
{
    private const string TituloValido = "Natal Solidário";
    private const string DescricaoValida = "Arrecadação de brinquedos para as crianças acolhidas";
    private const decimal MetaValida = 5000m;

    private static readonly DateTime DataInicioValida = DateTime.UtcNow.AddDays(-1);
    private static readonly DateTime DataFimValida = DateTime.UtcNow.AddDays(30);

    private readonly Mock<ICampanhaRepository> _campanhaRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CriarCampanhaUseCase _useCase;

    public CriarCampanhaUseCaseTests()
    {
        _campanhaRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _useCase = new CriarCampanhaUseCase(_campanhaRepositoryMock.Object);
    }

    [Fact]
    public async Task AoCriarCampanhaComDadosValidosDeveAdicionarERetornarId()
    {
        // Arrange
        CriarCampanhaRequest request = new(TituloValido, DescricaoValida, DataInicioValida, DataFimValida, MetaValida);

        // Act
        Guid id = await _useCase.Executar(request);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        _campanhaRepositoryMock.Verify(r => r.Adicionar(It.Is<Campanha>(c =>
            c.Titulo == TituloValido && c.MetaFinanceira == MetaValida)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task AoCriarCampanhaComDataFimNoPassadoDeveLancarValidationException()
    {
        // Arrange
        DateTime dataFimNoPassado = DateTime.UtcNow.AddDays(-1);
        CriarCampanhaRequest request = new(TituloValido, DescricaoValida, DataInicioValida, dataFimNoPassado, MetaValida);

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        Assert.Equal("Data de término da campanha não pode estar no passado", ex.Message);
        _campanhaRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Campanha>()), Times.Never);
    }

    [Fact]
    public async Task AoCriarCampanhaComMetaFinanceiraZeroDeveLancarValidationException()
    {
        // Arrange
        CriarCampanhaRequest request = new(TituloValido, DescricaoValida, DataInicioValida, DataFimValida, 0m);

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
        Assert.Equal("Meta financeira deve ser maior que zero", ex.Message);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task AoCriarCampanhaComTituloVazioDeveLancarValidationException()
    {
        // Arrange
        CriarCampanhaRequest request = new(string.Empty, DescricaoValida, DataInicioValida, DataFimValida, MetaValida);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));
    }
}
