using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Doacoes.Validators;
using FluentValidation.Results;

namespace ConexaoSolidaria.Tests.Doacoes.Application.Validators;

public class FiltroListagemDoacaoAdminRequestValidatorTests
{
    private readonly FiltroListagemDoacaoAdminRequestValidator _validator = new();

    [Fact]
    public void AoValidarFiltroAdminPadraoDeveSerValido()
    {
        ValidationResult resultado = _validator.Validate(new FiltroListagemDoacaoAdminRequest());

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AoValidarFiltroAdminComPaginaInvalidaDeveSerInvalido(int pagina)
    {
        FiltroListagemDoacaoAdminRequest filtro = new() { Pagina = pagina };

        ValidationResult resultado = _validator.Validate(filtro);

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void AoValidarFiltroAdminComIdDoadorPreenchidoDeveSerValido()
    {
        FiltroListagemDoacaoAdminRequest filtro = new() { IdDoador = Guid.NewGuid() };

        ValidationResult resultado = _validator.Validate(filtro);

        Assert.True(resultado.IsValid);
    }
}
