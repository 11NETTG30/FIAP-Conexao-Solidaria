namespace FCG.Application.Campanhas.DTOs;

public record CriarCampanhaRequest(
    string Titulo,
    string Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    decimal MetaFinanceira
);
