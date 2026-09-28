using System.Text.RegularExpressions;
using ConexaoSolidaria.Domain.Shared.Abstractions;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Domain.Identidade.ValueObjects;

public sealed class Cpf : ValueObject
{
    public string Valor { get; }

    public const byte TAMANHO_CPF = 11;

    private static readonly Regex naoDigitoRegex = new(
        @"\D",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(200)
    );

    public Cpf(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValidationException("CPF não pode ser vazio");

        string cpfNormalizado = naoDigitoRegex.Replace(valor, string.Empty);

        if (!EhValido(cpfNormalizado))
            throw new ValidationException("O CPF informado é inválido.");

        Valor = cpfNormalizado;
    }

    public override string ToString() => Valor;

    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }

    public static bool EhValido(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        string cpf = naoDigitoRegex.Replace(valor, string.Empty);

        if (cpf.Length != TAMANHO_CPF)
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        int[] digitos = cpf.Select(c => c - '0').ToArray();

        int primeiroDigitoVerificador = CalcularDigitoVerificador(digitos, 9);
        if (primeiroDigitoVerificador != digitos[9])
            return false;

        int segundoDigitoVerificador = CalcularDigitoVerificador(digitos, 10);
        return segundoDigitoVerificador == digitos[10];
    }

    private static int CalcularDigitoVerificador(int[] digitos, int quantidadeDigitos)
    {
        int soma = 0;
        int multiplicador = quantidadeDigitos + 1;

        for (int i = 0; i < quantidadeDigitos; i++)
            soma += digitos[i] * (multiplicador - i);

        int resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }
}
