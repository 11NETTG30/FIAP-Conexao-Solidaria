using ConexaoSolidaria.Application.Doacoes.DTOs;
using FluentValidation;

namespace ConexaoSolidaria.Application.Doacoes.Validators;

public sealed class CriarDoacaoRequestValidator : AbstractValidator<CriarDoacaoRequest>
{
    public CriarDoacaoRequestValidator()
    {
        RuleFor(request => request.IdCampanha)
            .NotEmpty();

        RuleFor(request => request.ValorDoacao)
            .GreaterThan(0)
            .PrecisionScale(18, 2, true);
    }
}
