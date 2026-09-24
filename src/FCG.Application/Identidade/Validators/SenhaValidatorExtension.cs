using FluentValidation;

namespace FCG.Application.Identidade.Validators;

public static class SenhaValidatorExtension
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> SenhaValida()
        {
            return ruleBuilder
                .NotEmpty()
                .Length(8, 128)
                .Must(s => s.Any(char.IsUpper))
                .WithMessage("'{PropertyName}' deve conter ao menos uma letra maiúscula.")
                .Must(s => s.Any(char.IsLower))
                .WithMessage("'{PropertyName}' deve conter ao menos uma letra minúscula.")
                .Must(s => s.Any(char.IsDigit))
                .WithMessage("'{PropertyName}' deve conter ao menos um número.")
                .Must(s => s.Any(c => "!@#$%^&*()".Contains(c)))
                .WithMessage("'{PropertyName}' deve conter ao menos um caractere especial");
        }
    }
}