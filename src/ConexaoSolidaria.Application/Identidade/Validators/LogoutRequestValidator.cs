using ConexaoSolidaria.Application.Identidade.DTOs;
using FluentValidation;

namespace ConexaoSolidaria.Application.Identidade.Validators;

public class LogoutRequestValidator : AbstractValidator<RefreshRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}