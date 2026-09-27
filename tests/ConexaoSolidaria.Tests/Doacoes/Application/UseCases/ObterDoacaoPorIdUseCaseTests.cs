using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Doacoes.UseCases;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using Moq;

namespace ConexaoSolidaria.Tests.Doacoes.Application.UseCases;

public class ObterDoacaoPorIdUseCaseTests
{
    private static readonly Guid IdCampanha = Guid.NewGuid();
    private static readonly Guid IdDoador = Guid.NewGuid();
    private const decimal ValorValido = 250m;

    private readonly Mock<IDoacaoRepository> _doacaoRepositoryMock = new();
    private readonly Mock<IInformacoesUsuarioLogado> _informacoesUsuarioLogadoMock = new();
    private readonly ObterDoacaoPorIdUseCase _useCase;

    public ObterDoacaoPorIdUseCaseTests()
    {
        _informacoesUsuarioLogadoMock.Setup(i => i.Id).Returns(IdDoador);

        _useCase = new ObterDoacaoPorIdUseCase(
            _doacaoRepositoryMock.Object,
            _informacoesUsuarioLogadoMock.Object);
    }

    [Fact]
    public async Task AoObterDoacaoDoProprioDoadorDeveRetornarDto()
    {
        Doacao doacao = new(IdCampanha, IdDoador, ValorValido);
        _doacaoRepositoryMock.Setup(r => r.ObterPorId(doacao.Id)).ReturnsAsync(doacao);

        DoacaoDto dto = await _useCase.Executar(doacao.Id);

        Assert.Equal(doacao.Id, dto.Id);
        Assert.Equal(IdCampanha, dto.IdCampanha);
        Assert.Equal(ValorValido, dto.ValorDoacao);
    }

    [Fact]
    public async Task AoObterDoacaoDeOutroDoadorDeveLancarNotFoundException()
    {
        Doacao doacao = new(IdCampanha, Guid.NewGuid(), ValorValido);
        _doacaoRepositoryMock.Setup(r => r.ObterPorId(doacao.Id)).ReturnsAsync(doacao);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Executar(doacao.Id));
    }

    [Fact]
    public async Task AoObterDoacaoInexistenteDeveLancarNotFoundException()
    {
        Guid idInexistente = Guid.NewGuid();
        _doacaoRepositoryMock.Setup(r => r.ObterPorId(idInexistente)).ReturnsAsync((Doacao?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Executar(idInexistente));
    }
}
