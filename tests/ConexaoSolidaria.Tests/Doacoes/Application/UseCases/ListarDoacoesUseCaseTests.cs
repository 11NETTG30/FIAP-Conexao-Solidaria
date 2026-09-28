using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Doacoes.UseCases;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Enums;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using Moq;

namespace ConexaoSolidaria.Tests.Doacoes.Application.UseCases;

public class ListarDoacoesUseCaseTests
{
    private static readonly Guid IdDoadorLogado = Guid.NewGuid();
    private static readonly Guid IdCampanha = Guid.NewGuid();

    private readonly Mock<IDoacaoRepository> _doacaoRepositoryMock = new();
    private readonly Mock<IInformacoesUsuarioLogado> _informacoesUsuarioLogadoMock = new();
    private readonly ListarDoacoesUseCase _useCase;

    public ListarDoacoesUseCaseTests()
    {
        _informacoesUsuarioLogadoMock.Setup(i => i.Id).Returns(IdDoadorLogado);

        _useCase = new ListarDoacoesUseCase(
            _doacaoRepositoryMock.Object,
            _informacoesUsuarioLogadoMock.Object);
    }

    private static Doacao CriarDoacao(Guid idDoador, decimal valor = 100m) =>
        new(IdCampanha, idDoador, valor);

    [Fact]
    public async Task AoListarDoacoesDeveFiltrarSemprePeloDoadorLogado()
    {
        _doacaoRepositoryMock
            .Setup(r => r.Listar(It.Is<FiltroListagemDoacao>(f => f.IdDoador == IdDoadorLogado)))
            .ReturnsAsync(new ResultadoPaginado<Doacao>([], 0));

        await _useCase.Executar(new FiltroListagemDoacaoRequest());

        _doacaoRepositoryMock.Verify(r => r.Listar(It.Is<FiltroListagemDoacao>(f => f.IdDoador == IdDoadorLogado)), Times.Once);
    }

    [Fact]
    public async Task AoListarDoacoesDeveRetornarPaginaComItensMapeadosETotalDoRepositorio()
    {
        List<Doacao> doacoes = [CriarDoacao(IdDoadorLogado), CriarDoacao(IdDoadorLogado)];
        _doacaoRepositoryMock
            .Setup(r => r.Listar(It.IsAny<FiltroListagemDoacao>()))
            .ReturnsAsync(new ResultadoPaginado<Doacao>(doacoes, TotalItens: 5));

        PaginaDto<DoacaoDto> pagina = await _useCase.Executar(new FiltroListagemDoacaoRequest());

        Assert.Equal(2, pagina.Itens.Count());
        Assert.Equal(5, pagina.TotalItens);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanhoPagina);
    }

    [Fact]
    public async Task AoListarDoacoesComFiltrosDeveRepassarStatusCampanhaEDatasParaORepositorio()
    {
        DateTime dataInicio = DateTime.UtcNow.AddDays(-7);
        DateTime dataFim = DateTime.UtcNow;
        FiltroListagemDoacaoRequest request = new()
        {
            Status = StatusDoacao.Confirmada,
            IdCampanha = IdCampanha,
            DataInicio = dataInicio,
            DataFim = dataFim,
            Pagina = 2,
            TamanhoPagina = 10
        };

        _doacaoRepositoryMock
            .Setup(r => r.Listar(It.Is<FiltroListagemDoacao>(f =>
                f.Status == StatusDoacao.Confirmada &&
                f.IdCampanha == IdCampanha &&
                f.DataInicio == dataInicio &&
                f.DataFim == dataFim &&
                f.Pagina == 2 &&
                f.TamanhoPagina == 10)))
            .ReturnsAsync(new ResultadoPaginado<Doacao>([], 0));

        await _useCase.Executar(request);

        _doacaoRepositoryMock.VerifyAll();
    }
}
