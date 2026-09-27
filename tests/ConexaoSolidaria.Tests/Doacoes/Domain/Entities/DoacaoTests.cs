using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Enums;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Tests.Doacoes.Domain.Entities;

public class DoacaoTests
{
    private static readonly Guid IdCampanhaValida = Guid.NewGuid();
    private static readonly Guid IdDoadorValido = Guid.NewGuid();
    private const decimal ValorValido = 100m;

    [Fact]
    public void AoCriarDoacaoComDadosValidosDeveIniciarPendente()
    {
        Doacao doacao = CriarDoacaoValida();

        Assert.Equal(IdCampanhaValida, doacao.IdCampanha);
        Assert.Equal(IdDoadorValido, doacao.IdDoador);
        Assert.Equal(ValorValido, doacao.ValorDoacao);
        Assert.Equal(StatusDoacao.Pendente, doacao.Status);
        Assert.NotEqual(Guid.Empty, doacao.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void AoCriarDoacaoComValorMenorOuIgualAZeroDeveLancarExcecao(decimal valor)
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Doacao(IdCampanhaValida, IdDoadorValido, valor));

        Assert.Equal("Valor da doação deve ser maior que zero", ex.Message);
    }

    [Fact]
    public void AoCriarDoacaoComValorComMaisDeDuasCasasDecimaisDeveLancarExcecao()
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Doacao(IdCampanhaValida, IdDoadorValido, 10.555m));

        Assert.Equal("Valor da doação deve ter no máximo 2 casas decimais", ex.Message);
    }

    [Fact]
    public void AoCriarDoacaoComIdCampanhaVazioDeveLancarExcecao()
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Doacao(Guid.Empty, IdDoadorValido, ValorValido));

        Assert.Equal("Id da campanha é obrigatório", ex.Message);
    }

    [Fact]
    public void AoCriarDoacaoComIdDoadorVazioDeveLancarExcecao()
    {
        ValidationException ex = Assert.Throws<ValidationException>(() =>
            new Doacao(IdCampanhaValida, Guid.Empty, ValorValido));

        Assert.Equal("Id do doador é obrigatório", ex.Message);
    }

    [Fact]
    public void AoConfirmarDoacaoPendenteDeveAlterarStatus()
    {
        Doacao doacao = CriarDoacaoValida();

        doacao.Confirmar();

        Assert.Equal(StatusDoacao.Confirmada, doacao.Status);
    }

    [Fact]
    public void AoRejeitarDoacaoPendenteDeveAlterarStatus()
    {
        Doacao doacao = CriarDoacaoValida();

        doacao.Rejeitar();

        Assert.Equal(StatusDoacao.Rejeitada, doacao.Status);
    }

    [Fact]
    public void AoConfirmarDoacaoJaProcessadaDeveLancarExcecao()
    {
        Doacao doacao = CriarDoacaoValida();
        doacao.Confirmar();

        ConflictException ex = Assert.Throws<ConflictException>(doacao.Confirmar);

        Assert.Equal("Doação já foi processada", ex.Message);
    }

    [Fact]
    public void AoRejeitarDoacaoJaProcessadaDeveLancarExcecao()
    {
        Doacao doacao = CriarDoacaoValida();
        doacao.Rejeitar();

        ConflictException ex = Assert.Throws<ConflictException>(doacao.Rejeitar);

        Assert.Equal("Doação já foi processada", ex.Message);
    }
    
    private static Doacao CriarDoacaoValida() =>
        new(IdCampanhaValida, IdDoadorValido, ValorValido);
}
