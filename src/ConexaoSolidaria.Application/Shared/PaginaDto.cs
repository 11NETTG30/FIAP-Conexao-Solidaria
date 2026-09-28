namespace ConexaoSolidaria.Application.Shared;

/// <summary>
/// DTO genérico de resposta paginada
/// </summary>
public record PaginaDto<T>(
    IEnumerable<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens
);
