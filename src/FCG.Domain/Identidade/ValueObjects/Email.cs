using System.Text.RegularExpressions;
using FCG.Domain.Shared.Abstractions;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Domain.Identidade.ValueObjects;

public sealed class Email : ValueObject
{
    public string Valor { get; }
    
    public const short TAMANHO_MAXIMO_EMAIL = 256;
    
    public static readonly Regex emailRegex = new(
        @"^[a-z0-9]+([._-][a-z0-9]+)*@([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z0-9]{2,}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(200)
    );

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValidationException("E-mail não pode ser vazio");

        string emailNormalizado = valor.Trim().ToLowerInvariant();
        
        if (emailNormalizado.Length > TAMANHO_MAXIMO_EMAIL)
            throw new ValidationException($"E-mail muito longo, excedeu {TAMANHO_MAXIMO_EMAIL} caracteres");
        
        bool emailValido = ValidarEmail(emailNormalizado);
        
        if (!emailValido)
            throw new ValidationException("O e-mail informado é inválido.");

        Valor = emailNormalizado;
    }
    
    public override string ToString() => Valor;
    
    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }
    
    private static bool ValidarEmail(string email) => emailRegex.IsMatch(email);
}