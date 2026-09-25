using ConexaoSolidaria.Domain.Campanhas.Enums;

namespace ConexaoSolidaria.Application.Campanhas.DTOs;

// Todos os campos são opcionais: só o que vier preenchido é alterado.
public record EditarCampanhaRequest(
    string? Titulo,
    string? Descricao,
    DateTime? DataInicio,
    DateTime? DataFim,
    decimal? MetaFinanceira,
    StatusCampanha? Status
);
