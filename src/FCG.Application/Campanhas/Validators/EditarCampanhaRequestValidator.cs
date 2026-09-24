using FCG.Application.Campanhas.DTOs;
using FluentValidation;

namespace FCG.Application.Campanhas.Validators;

// Campos opcionais: cada regra só vale quando o campo vier preenchido.
public sealed class EditarCampanhaRequestValidator : AbstractValidator<EditarCampanhaRequest>
{
    public EditarCampanhaRequestValidator()
    {
        RuleFor(request => request.Titulo)
            .NotEmpty()
            .Length(3, 150)
            .When(request => request.Titulo is not null);

        RuleFor(request => request.Descricao)
            .NotEmpty()
            .MaximumLength(2000)
            .When(request => request.Descricao is not null);

        RuleFor(request => request.DataFim)
            .GreaterThan(request => request.DataInicio)
            .WithMessage("'Data Fim' deve ser posterior à data de início.")
            .When(request => request.DataInicio is not null && request.DataFim is not null);

        RuleFor(request => request.MetaFinanceira)
            .GreaterThan(0)
            .When(request => request.MetaFinanceira is not null);

        RuleFor(request => request.Status)
            .IsInEnum()
            .When(request => request.Status is not null);
    }
}
