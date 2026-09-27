namespace ConexaoSolidaria.Application.Doacoes.DTOs;

public record CriarDoacaoRequest(
    Guid IdCampanha,
    decimal ValorDoacao
);
