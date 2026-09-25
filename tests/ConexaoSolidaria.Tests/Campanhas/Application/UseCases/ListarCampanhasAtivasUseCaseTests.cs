using ConexaoSolidaria.Application.Campanhas.DTOs;
using ConexaoSolidaria.Application.Campanhas.UseCases;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Repositories;
using Moq;

namespace ConexaoSolidaria.Tests.Campanhas.Application.UseCases;

public class ListarCampanhasAtivasUseCaseTests
{
    private readonly Mock<ICampanhaRepository> _campanhaRepositoryMock = new();
    private readonly ListarCampanhasAtivasUseCase _useCase;

    public ListarCampanhasAtivasUseCaseTests()
    {
        _useCase = new ListarCampanhasAtivasUseCase(_campanhaRepositoryMock.Object);
    }

    [Fact]
    public async Task AoListarCampanhasAtivasSemCampanhasDeveRetornarListaVazia()
    {
        // Arrange
        _campanhaRepositoryMock.Setup(r => r.ListarAtivas()).ReturnsAsync([]);

        // Act
        IEnumerable<CampanhaAtivaDto> resultado = await _useCase.Executar();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task AoListarCampanhasAtivasDeveMapearParaCampanhaAtivaDtoSemExporCamposInternos()
    {
        // Arrange
        Campanha campanha = new(
            "Natal Solidário",
            "Arrecadação de brinquedos",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30),
            5000m);

        _campanhaRepositoryMock.Setup(r => r.ListarAtivas()).ReturnsAsync([campanha]);

        // Act
        IEnumerable<CampanhaAtivaDto> resultado = await _useCase.Executar();

        // Assert
        CampanhaAtivaDto dto = Assert.Single(resultado);
        Assert.Equal(campanha.Id, dto.Id);
        Assert.Equal(campanha.Titulo, dto.Titulo);
        Assert.Equal(campanha.MetaFinanceira, dto.MetaFinanceira);
        Assert.Equal(campanha.ValorArrecadado, dto.ValorArrecadado);
    }
}
