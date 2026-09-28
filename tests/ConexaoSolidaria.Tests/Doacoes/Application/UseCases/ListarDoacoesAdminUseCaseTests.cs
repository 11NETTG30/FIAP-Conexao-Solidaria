using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Doacoes.UseCases;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Enums;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using Moq;

namespace ConexaoSolidaria.Tests.Doacoes.Application.UseCases;

public class ListarDoacoesAdminUseCaseTests
{
    private static readonly Guid IdDoadorA = Guid.NewGuid();
    private static readonly Guid IdDoadorB = Guid.NewGuid();
    private static readonly Guid IdCampanha = Guid.NewGuid();

    private readonly Mock<IDoacaoRepository> _doacaoRepositoryMock = new();
    private readonly ListarDoacoesAdminUseCase _useCase;

    public ListarDoacoesAdminUseCaseTests()
    {
        _useCase = new ListarDoacoesAdminUseCase(_doacaoRepositoryMock.Object);
    }

    private static Doacao CriarDoacao(Guid idDoador, decimal valor = 100m) =>
        new(IdCampanha, idDoador, valor);

    [Fact]
    public async Task AoListarDoacoesSemFiltroDeIdDoadorDeveRetornarDoacoesDeMultiplosDoadores()
    {
        List<Doacao> doacoes = [CriarDoacao(IdDoadorA), CriarDoacao(IdDoadorB)];
        _doacaoRepositoryMock
            .Setup(r => r.Listar(It.Is<FiltroListagemDoacao>(f => f.IdDoador == null)))
            .ReturnsAsync(new ResultadoPaginado<Doacao>(doacoes, 2));

        PaginaDto<DoacaoDto> pagina = await _useCase.Executar(new FiltroListagemDoacaoAdminRequest());

        Assert.Equal(2, pagina.Itens.Count());
        Assert.Contains(pagina.Itens, d => d.IdDoador == IdDoadorA);
        Assert.Contains(pagina.Itens, d => d.IdDoador == IdDoadorB);
    }

    [Fact]
    public async Task AoListarDoacoesComIdDoadorEspecificoDeveRepassarFiltroParaORepositorio()
    {
        FiltroListagemDoacaoAdminRequest request = new() { IdDoador = IdDoadorA };
        _doacaoRepositoryMock
            .Setup(r => r.Listar(It.Is<FiltroListagemDoacao>(f => f.IdDoador == IdDoadorA)))
            .ReturnsAsync(new ResultadoPaginado<Doacao>([CriarDoacao(IdDoadorA)], 1));

        await _useCase.Executar(request);

        _doacaoRepositoryMock.Verify(r => r.Listar(It.Is<FiltroListagemDoacao>(f => f.IdDoador == IdDoadorA)), Times.Once);
    }

    [Fact]
    public async Task AoListarDoacoesComFiltroDeStatusDeveRepassarParaORepositorio()
    {
        FiltroListagemDoacaoAdminRequest request = new() { Status = StatusDoacao.Rejeitada };
        _doacaoRepositoryMock
            .Setup(r => r.Listar(It.Is<FiltroListagemDoacao>(f => f.Status == StatusDoacao.Rejeitada)))
            .ReturnsAsync(new ResultadoPaginado<Doacao>([], 0));

        await _useCase.Executar(request);

        _doacaoRepositoryMock.Verify(r => r.Listar(It.Is<FiltroListagemDoacao>(f => f.Status == StatusDoacao.Rejeitada)), Times.Once);
    }
}
