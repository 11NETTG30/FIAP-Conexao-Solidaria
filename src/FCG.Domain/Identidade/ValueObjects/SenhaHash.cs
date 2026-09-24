using FCG.Domain.Shared.Abstractions;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Domain.Identidade.ValueObjects;

public sealed class SenhaHash : ValueObject
{
    public const byte TAMANHO_ESPERADO_SENHA_HASH = 69;
    public string Senha { get; }

    public SenhaHash(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ValidationException("Senha não pode ser vazia");

        if (senha.Length != TAMANHO_ESPERADO_SENHA_HASH)
            throw new ValidationException("Senha hash com tamanho inesperado");
        
        Senha = senha;
    }

    public override string ToString() => Senha;
    
    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Senha;
    }
}