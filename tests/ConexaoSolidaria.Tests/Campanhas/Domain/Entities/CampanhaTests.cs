using ConexaoSolidaria.Domain.Campanhas.Entities;
using ConexaoSolidaria.Domain.Campanhas.Enums;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Tests.Campanhas.Domain.Entities;

public class CampanhaTests
{
    private const string TituloValido = "Natal Solidário";
    private const string DescricaoValida = "Arrecadação de brinquedos para as crianças acolhidas";
    private const decimal MetaValida = 5000m;

    private static readonly DateTime DataInicioValida = DateTime.UtcNow.AddDays(-1);
    private static readonly DateTime DataFimValida = DateTime.UtcNow.AddDays(30);

    private static Campanha CriarCampanhaValida() =>
        new(TituloValido, DescricaoValida, DataInicioValida, DataFimValida, MetaValida);

    [Fact]
    public void AoCriarCampanhaComDadosValidosDeveIniciarAtivaESemValorArrecadado()
    {
        Campanha campanha = CriarCampanhaValida();

        Assert.Equal(TituloValido, campanha.Titulo);
        Assert.Equal(DescricaoValida, campanha.Descricao);
        Assert.Equal(DataInicioValida, campanha.DataInicio);
        Assert.Equal(DataFimValida, campanha.DataFim);
        Assert.Equal(MetaValida, campanha.MetaFinanceira);
        Assert.Equal(StatusCampanha.Ativa, campanha.Status);
        Assert.Equal(0m, campanha.ValorArrecadado);
        Assert.NotEqual(Guid.Empty, campanha.Id);
    }

    [Fact]
    public void AoCriarCampanhaComDataFimNoPassadoDeveLancarExcecao()
    {
        DateTime dataInicio = DateTime.UtcNow.AddDays(-10);
        DateTime dataFim = DateTime.UtcNow.AddDays(-1);

        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Campanha(TituloValido, DescricaoValida, dataInicio, dataFim, MetaValida));

        Assert.Equal("Data de término da campanha não pode estar no passado", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void AoCriarCampanhaComMetaFinanceiraMenorOuIgualAZeroDeveLancarExcecao(decimal meta)
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Campanha(TituloValido, DescricaoValida, DataInicioValida, DataFimValida, meta));

        Assert.Equal("Meta financeira deve ser maior que zero", ex.Message);
    }

    [Fact]
    public void AoCriarCampanhaComDataFimAnteriorADataInicioDeveLancarExcecao()
    {
        DateTime dataInicio = DateTime.UtcNow.AddDays(20);
        DateTime dataFim = DateTime.UtcNow.AddDays(10);

        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Campanha(TituloValido, DescricaoValida, dataInicio, dataFim, MetaValida));

        Assert.Equal("Data de término deve ser posterior à data de início", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AoCriarCampanhaComTituloVazioDeveLancarExcecao(string? titulo)
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Campanha(titulo!, DescricaoValida, DataInicioValida, DataFimValida, MetaValida));

        Assert.Equal("Título não pode ser vazio ou nulo", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AoCriarCampanhaComDescricaoVaziaDeveLancarExcecao(string? descricao)
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Campanha(TituloValido, descricao!, DataInicioValida, DataFimValida, MetaValida));

        Assert.Equal("Descrição não pode ser vazia ou nula", ex.Message);
    }

    [Fact]
    public void AoCriarCampanhaComDataSemFusoDeveTratarComoUtc()
    {
        DateTime dataFimSemFuso = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(30), DateTimeKind.Unspecified);

        Campanha campanha = new(TituloValido, DescricaoValida, DataInicioValida, dataFimSemFuso, MetaValida);

        Assert.Equal(DateTimeKind.Utc, campanha.DataFim.Kind);
    }

    [Theory]
    [InlineData(StatusCampanha.Concluida)]
    [InlineData(StatusCampanha.Cancelada)]
    public void AoAlterarStatusDeveAtualizarCampanha(StatusCampanha status)
    {
        Campanha campanha = CriarCampanhaValida();

        campanha.SetStatus(status);

        Assert.Equal(status, campanha.Status);
    }

    [Fact]
    public void AoAlterarStatusParaValorNaoDefinidoDeveLancarExcecao()
    {
        Campanha campanha = CriarCampanhaValida();

        ValidationException ex = Assert.Throws<ValidationException>(() =>
            campanha.SetStatus((StatusCampanha)99));

        Assert.Equal("Status da campanha é inválido, valor não definido", ex.Message);
    }

    [Theory]
    [InlineData(10.555)]
    [InlineData(0.001)]
    public void AoCriarCampanhaComMetaFinanceiraComMaisDeDuasCasasDecimaisDeveLancarExcecao(decimal meta)
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Campanha(TituloValido, DescricaoValida, DataInicioValida, DataFimValida, meta));

        Assert.Equal("Meta financeira deve ter no máximo 2 casas decimais", ex.Message);
    }

    // Literais decimal (e não double do InlineData) para os zeros à direita
    // chegarem de fato ao teste
    public static TheoryData<decimal> MetasComAteDuasCasasDecimais => [10m, 10.5m, 10.55m, 10.500m];

    [Theory]
    [MemberData(nameof(MetasComAteDuasCasasDecimais))]
    public void AoCriarCampanhaComMetaFinanceiraComAteDuasCasasDecimaisDeveAceitar(decimal meta)
    {
        Campanha campanha = new(TituloValido, DescricaoValida, DataInicioValida, DataFimValida, meta);

        Assert.Equal(meta, campanha.MetaFinanceira);
    }

    [Fact]
    public void AoVerificarEdicaoDeCampanhaAtivaNaoDeveLancarExcecao()
    {
        Campanha campanha = CriarCampanhaValida();

        Exception? ex = Record.Exception(campanha.GarantirQuePodeSerEditada);

        Assert.Null(ex);
    }

    [Theory]
    [InlineData(StatusCampanha.Concluida)]
    [InlineData(StatusCampanha.Cancelada)]
    public void AoVerificarEdicaoDeCampanhaEncerradaDeveLancarExcecao(StatusCampanha status)
    {
        Campanha campanha = CriarCampanhaValida();
        campanha.SetStatus(status);

        ValidationException ex = Assert.Throws<ValidationException>(campanha.GarantirQuePodeSerEditada);

        Assert.Equal("Campanha concluída ou cancelada não pode ser editada", ex.Message);
    }

    [Fact]
    public void AoEditarPeriodoComDataFimNoPassadoDeveLancarExcecao()
    {
        Campanha campanha = CriarCampanhaValida();

        ValidationException ex = Assert.Throws<ValidationException>(() =>
            campanha.SetPeriodo(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1)));

        Assert.Equal("Data de término da campanha não pode estar no passado", ex.Message);
        Assert.Equal(DataFimValida, campanha.DataFim);
    }

    [Fact]
    public void AoCriarCampanhaComDataInicioNoFuturoDeveAceitar()
    {
        DateTime dataInicio = DateTime.UtcNow.AddDays(5);

        Campanha campanha = new(TituloValido, DescricaoValida, dataInicio, DataFimValida, MetaValida);

        Assert.Equal(dataInicio, campanha.DataInicio);
    }

    [Fact]
    public void AoEditarMetaFinanceiraParaZeroDeveLancarExcecao()
    {
        Campanha campanha = CriarCampanhaValida();

        Assert.Throws<ValidationException>(() => campanha.SetMetaFinanceira(0));
        Assert.Equal(MetaValida, campanha.MetaFinanceira);
    }
}
