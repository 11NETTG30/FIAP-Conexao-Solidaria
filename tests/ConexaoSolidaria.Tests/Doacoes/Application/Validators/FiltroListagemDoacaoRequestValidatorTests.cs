using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Doacoes.Validators;
using FluentValidation.Results;

namespace ConexaoSolidaria.Tests.Doacoes.Application.Validators;

public class FiltroListagemDoacaoRequestValidatorTests
{
    private readonly FiltroListagemDoacaoRequestValidator _validator = new();

    [Fact]
    public void AoValidarFiltroPadraoDeveSerValido()
    {
        ValidationResult resultado = _validator.Validate(new FiltroListagemDoacaoRequest());

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AoValidarFiltroComPaginaInvalidaDeveSerInvalido(int pagina)
    {
        FiltroListagemDoacaoRequest filtro = new() { Pagina = pagina };

        ValidationResult resultado = _validator.Validate(filtro);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(FiltroListagemDoacaoRequest.Pagina));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void AoValidarFiltroComTamanhoPaginaInvalidoDeveSerInvalido(int tamanhoPagina)
    {
        FiltroListagemDoacaoRequest filtro = new() { TamanhoPagina = tamanhoPagina };

        ValidationResult resultado = _validator.Validate(filtro);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(FiltroListagemDoacaoRequest.TamanhoPagina));
    }

    [Fact]
    public void AoValidarFiltroComDataFimAnteriorADataInicioDeveSerInvalido()
    {
        FiltroListagemDoacaoRequest filtro = new()
        {
            DataInicio = DateTime.UtcNow,
            DataFim = DateTime.UtcNow.AddDays(-1)
        };

        ValidationResult resultado = _validator.Validate(filtro);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(FiltroListagemDoacaoRequest.DataFim));
    }

    [Fact]
    public void AoValidarFiltroComDataFimIgualADataInicioDeveSerValido()
    {
        DateTime data = DateTime.UtcNow;
        FiltroListagemDoacaoRequest filtro = new() { DataInicio = data, DataFim = data };

        ValidationResult resultado = _validator.Validate(filtro);

        Assert.True(resultado.IsValid);
    }
}
