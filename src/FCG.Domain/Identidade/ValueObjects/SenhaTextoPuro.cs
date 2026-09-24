using FCG.Domain.Shared.Abstractions;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Domain.Identidade.ValueObjects;

public sealed class SenhaTextoPuro : ValueObject
{
    public const byte TAMANHO_MINIMO_SENHA = 8;
    public const byte TAMANHO_MAXIMO_SENHA = 128;
    public string Senha { get; }

    public SenhaTextoPuro(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ValidationException("Senha obrigatória");
            
        switch (senha.Length)
        {
            case < TAMANHO_MINIMO_SENHA:
                throw new ValidationException("Senha deve ter no mínimo 8 caracteres");
            case > TAMANHO_MAXIMO_SENHA:
                throw new ValidationException("Senha muito longa");
        }

        if (!senha.Any(char.IsUpper))
            throw new ValidationException("A senha deve conter ao menos uma letra maiúscula");
            
        if (!senha.Any(char.IsLower))
            throw new ValidationException("A senha deve conter ao menos uma letra minúscula");
            
        if (!senha.Any(char.IsDigit))
            throw new ValidationException("A senha deve conter ao menos um número");
            
        if (!senha.Any(c => "!@#$%^&*()".Contains(c)))
            throw new ValidationException("A senha deve conter ao menos um caractere especial");
        
        Senha = senha;
    }

    public SenhaTextoPuro(string senha, string confirmacaoSenha) : this(senha)
    {
        if(senha != confirmacaoSenha)
            throw new ValidationException("'Senha' e 'Confirmação de Senha' são diferentes");
    }

    public override string ToString() => Senha;
    
    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Senha;
    }
}