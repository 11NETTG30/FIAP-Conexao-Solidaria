using FCG.Domain.Campanhas.Entities;

namespace FCG.Application.Campanhas.DTOs;

// Id vai junto para o doador conseguir informar o IdCampanha na doação.
public record CampanhaAtivaDto(
    Guid Id,
    string Titulo,
    decimal MetaFinanceira,
    decimal ValorArrecadado
)
{
    public static explicit operator CampanhaAtivaDto(Campanha campanha)
    {
        return new CampanhaAtivaDto(
            Id: campanha.Id,
            Titulo: campanha.Titulo,
            MetaFinanceira: campanha.MetaFinanceira,
            ValorArrecadado: campanha.ValorArrecadado
        );
    }
}
