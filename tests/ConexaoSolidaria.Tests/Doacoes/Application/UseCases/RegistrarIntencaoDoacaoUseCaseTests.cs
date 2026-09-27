using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Doacoes.UseCases;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Enums;
using ConexaoSolidaria.Domain.Campanhas.Repositories;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Events;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Doacoes.Application.UseCases;

public class RegistrarIntencaoDoacaoUseCaseTests
{
    private static readonly Guid IdCampanha = Guid.NewGuid();
    private static readonly Guid IdDoador = Guid.NewGuid();
    private const decimal ValorValido = 250m;

    private readonly Mock<IDoacaoRepository> _doacaoRepositoryMock = new();
    private readonly Mock<ICampanhaRepository> _campanhaRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IInformacoesUsuarioLogado> _informacoesUsuarioLogadoMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly RegistrarIntencaoDoacaoUseCase _useCase;

    public RegistrarIntencaoDoacaoUseCaseTests()
    {
        _doacaoRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _informacoesUsuarioLogadoMock.Setup(i => i.Id).Returns(IdDoador);

        _useCase = new RegistrarIntencaoDoacaoUseCase(
            _doacaoRepositoryMock.Object,
            _campanhaRepositoryMock.Object,
            _informacoesUsuarioLogadoMock.Object,
            _eventPublisherMock.Object);
    }

    private static Campanha CriarCampanhaAtiva() =>
        new("Campanha válida", "Descrição", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30), 1000m);

    [Fact]
    public async Task AoRegistrarIntencaoDoacaoComDadosValidosDeveAdicionarComitarEPublicarEvento()
    {
        CriarDoacaoRequest request = new(IdCampanha, ValorValido);
        _campanhaRepositoryMock.Setup(r => r.ObterPorId(IdCampanha)).ReturnsAsync(CriarCampanhaAtiva());

        Guid id = await _useCase.Executar(request);

        Assert.NotEqual(Guid.Empty, id);
        _doacaoRepositoryMock.Verify(r => r.Adicionar(It.Is<Doacao>(d =>
            d.IdCampanha == IdCampanha && d.IdDoador == IdDoador && d.ValorDoacao == ValorValido)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _eventPublisherMock.Verify(p => p.Publicar(
            It.Is<DoacaoRecebidaEvent>(e => e.IdDoacao == id && e.IdCampanha == IdCampanha && e.ValorDoacao == ValorValido),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AoRegistrarIntencaoDoacaoComValorZeroDeveLancarValidationExceptionSemPublicarEvento()
    {
        CriarDoacaoRequest request = new(IdCampanha, 0m);
        _campanhaRepositoryMock.Setup(r => r.ObterPorId(IdCampanha)).ReturnsAsync(CriarCampanhaAtiva());

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Executar(request));

        _doacaoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Doacao>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        _eventPublisherMock.Verify(p => p.Publicar(It.IsAny<DoacaoRecebidaEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AoRegistrarIntencaoDoacaoComCampanhaInexistenteDeveLancarNotFoundExceptionSemPublicarEvento()
    {
        CriarDoacaoRequest request = new(IdCampanha, ValorValido);
        _campanhaRepositoryMock.Setup(r => r.ObterPorId(IdCampanha)).ReturnsAsync((Campanha?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Executar(request));

        _doacaoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Doacao>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        _eventPublisherMock.Verify(p => p.Publicar(It.IsAny<DoacaoRecebidaEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AoRegistrarIntencaoDoacaoComCampanhaEncerradaDeveLancarConflictExceptionSemPublicarEvento()
    {
        CriarDoacaoRequest request = new(IdCampanha, ValorValido);
        Campanha campanhaEncerrada = CriarCampanhaAtiva();
        campanhaEncerrada.SetStatus(StatusCampanha.Concluida);
        _campanhaRepositoryMock.Setup(r => r.ObterPorId(IdCampanha)).ReturnsAsync(campanhaEncerrada);

        await Assert.ThrowsAsync<ConflictException>(() => _useCase.Executar(request));

        _doacaoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Doacao>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        _eventPublisherMock.Verify(p => p.Publicar(It.IsAny<DoacaoRecebidaEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
