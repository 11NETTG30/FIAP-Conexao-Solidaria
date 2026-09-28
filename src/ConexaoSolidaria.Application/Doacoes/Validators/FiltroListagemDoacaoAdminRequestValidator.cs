using ConexaoSolidaria.Application.Doacoes.DTOs;
using FluentValidation;

namespace ConexaoSolidaria.Application.Doacoes.Validators;

public sealed class FiltroListagemDoacaoAdminRequestValidator : AbstractValidator<FiltroListagemDoacaoAdminRequest>
{
    public FiltroListagemDoacaoAdminRequestValidator()
    {
        Include(new FiltroListagemDoacaoRequestValidator());
    }
}
