using ConexaoSolidaria.Application.Campanhas.DTOs;
using FluentValidation;

namespace ConexaoSolidaria.Application.Campanhas.Validators;

public sealed class CriarCampanhaRequestValidator : AbstractValidator<CriarCampanhaRequest>
{
    public CriarCampanhaRequestValidator()
    {
        RuleFor(request => request.Titulo)
            .NotEmpty()
            .Length(3, 150);

        RuleFor(request => request.Descricao)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(request => request.DataInicio)
            .NotEmpty();

        RuleFor(request => request.DataFim)
            .NotEmpty()
            .GreaterThan(request => request.DataInicio)
            .WithMessage("'Data Fim' deve ser posterior à data de início.");

        RuleFor(request => request.MetaFinanceira)
            .GreaterThan(0)
            .PrecisionScale(18, 2, true);
    }
}
