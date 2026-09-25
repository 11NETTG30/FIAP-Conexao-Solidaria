using ConexaoSolidaria.Application.Identidade.DTOs;
using FluentValidation;

namespace ConexaoSolidaria.Application.Identidade.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}