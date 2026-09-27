using ConexaoSolidaria.Application.Doacoes.Services;
using static ConexaoSolidaria.Application.Doacoes.Services.DecisaoProcessamentoDoacao;

namespace ConexaoSolidaria.Tests.Doacoes.Application.Services;

public class DecisaoProcessamentoDoacaoTests
{
    [Fact]
    public void AoDecidirComDoacaoJaProcessadaDeveIgnorarIndependenteDaCampanha()
    {
        ResultadoProcessamentoDoacao resultado = DecisaoProcessamentoDoacao.Decidir(
            doacaoJaProcessada: true, campanhaAptaAReceberDoacao: true);

        Assert.Equal(ResultadoProcessamentoDoacao.Ignorada, resultado);
    }

    [Fact]
    public void AoDecidirComDoacaoJaProcessadaECampanhaInaptaDeveIgnorar()
    {
        ResultadoProcessamentoDoacao resultado = DecisaoProcessamentoDoacao.Decidir(
            doacaoJaProcessada: true, campanhaAptaAReceberDoacao: false);

        Assert.Equal(ResultadoProcessamentoDoacao.Ignorada, resultado);
    }

    [Fact]
    public void AoDecidirComDoacaoNovaECampanhaAptaDeveConfirmar()
    {
        ResultadoProcessamentoDoacao resultado = DecisaoProcessamentoDoacao.Decidir(
            doacaoJaProcessada: false, campanhaAptaAReceberDoacao: true);

        Assert.Equal(ResultadoProcessamentoDoacao.Confirmada, resultado);
    }

    [Fact]
    public void AoDecidirComDoacaoNovaECampanhaInaptaDeveRejeitar()
    {
        ResultadoProcessamentoDoacao resultado = DecisaoProcessamentoDoacao.Decidir(
            doacaoJaProcessada: false, campanhaAptaAReceberDoacao: false);

        Assert.Equal(ResultadoProcessamentoDoacao.Rejeitada, resultado);
    }
}