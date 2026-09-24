using FCG.Application.Identidade.DTOs;
using FluentValidation;

namespace FCG.Application.Identidade.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}