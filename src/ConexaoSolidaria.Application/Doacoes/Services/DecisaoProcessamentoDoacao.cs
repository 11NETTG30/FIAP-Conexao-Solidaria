namespace ConexaoSolidaria.Application.Doacoes.Services;

public static class DecisaoProcessamentoDoacao
{
    public static ResultadoProcessamentoDoacao Decidir(bool doacaoJaProcessada, bool campanhaAptaAReceberDoacao)
    {
        if (doacaoJaProcessada)
            return ResultadoProcessamentoDoacao.Ignorada;

        return campanhaAptaAReceberDoacao
            ? ResultadoProcessamentoDoacao.Confirmada
            : ResultadoProcessamentoDoacao.Rejeitada;
    }
    
    public enum ResultadoProcessamentoDoacao
    {
        Ignorada,
        Confirmada,
        Rejeitada
    }
}
