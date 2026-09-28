using ConexaoSolidaria.Application.Doacoes.DTOs;
using FluentValidation;

namespace ConexaoSolidaria.Application.Doacoes.Validators;

public sealed class FiltroListagemDoacaoRequestValidator : AbstractValidator<FiltroListagemDoacaoRequest>
{
    public FiltroListagemDoacaoRequestValidator()
    {
        RuleFor(filtro => filtro.Pagina)
            .GreaterThanOrEqualTo(1);

        RuleFor(filtro => filtro.TamanhoPagina)
            .InclusiveBetween(1, 100);

        RuleFor(filtro => filtro.DataFim)
            .GreaterThanOrEqualTo(filtro => filtro.DataInicio)
            .When(filtro => filtro.DataInicio is not null && filtro.DataFim is not null)
            .WithMessage("'Data Fim' deve ser posterior ou igual à 'Data Início'.");
    }
}
