using FCG.Application.Identidade.DTOs;
using FluentValidation;

namespace FCG.Application.Identidade.Validators;

public sealed class AlterarSenhaRequestValidator : AbstractValidator<AlterarSenhaRequest>
{
    public AlterarSenhaRequestValidator()
    {
        RuleFor(request => request.SenhaAtual)
            .NotEmpty();
        
        RuleFor(request => request.NovaSenha)
            .SenhaValida()
            .NotEqual(request => request.SenhaAtual)
            .WithMessage("'{PropertyName}' deve ser diferente da senha atual");
        
        RuleFor(request => request.ConfirmacaoNovaSenha)
            .NotEmpty()
            .Equal(request => request.NovaSenha)
            .WithMessage("As senhas não conferem");
    }
}