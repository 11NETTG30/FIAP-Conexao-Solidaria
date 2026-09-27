using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Enums;

namespace ConexaoSolidaria.Application.Doacoes.DTOs;

public record DoacaoDto(
    Guid Id,
    Guid IdCampanha,
    Guid IdDoador,
    decimal ValorDoacao,
    StatusDoacao Status,
    DateTime DataCriacao
)
{
    public static explicit operator DoacaoDto(Doacao doacao) =>
        new(doacao.Id, doacao.IdCampanha, doacao.IdDoador, doacao.ValorDoacao, doacao.Status, doacao.DataCriacao);
}