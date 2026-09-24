using FCG.Application.Identidade.DTOs;
using FluentValidation;

namespace FCG.Application.Identidade.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(request => request.Senha)
            .NotEmpty();
    }
}