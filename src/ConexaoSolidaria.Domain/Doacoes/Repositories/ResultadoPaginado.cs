namespace ConexaoSolidaria.Domain.Doacoes.Repositories;

public record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int TotalItens
);
