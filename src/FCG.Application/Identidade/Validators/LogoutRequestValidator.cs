using FCG.Application.Identidade.DTOs;
using FluentValidation;

namespace FCG.Application.Identidade.Validators;

public class LogoutRequestValidator : AbstractValidator<RefreshRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}