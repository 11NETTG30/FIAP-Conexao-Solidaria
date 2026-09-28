using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Tests.Identidade.Domain.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("11144477735")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    [InlineData("529.982.247-25")]
    public void AoCriarCpfComFormatoValidoDeveSerCriadoComSucesso(string valor)
    {
        // Act
        Cpf cpf = new(valor);

        // Assert
        Assert.Equal(11, cpf.Valor.Length);
        Assert.DoesNotContain(".", cpf.Valor);
        Assert.DoesNotContain("-", cpf.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AoCriarCpfVazioOuNuloDeveLancarValidationException(string? valor)
    {
        // Act & Assert
        ValidationException ex = Assert.Throws<ValidationException>(() => new Cpf(valor!));
        Assert.Equal("CPF não pode ser vazio", ex.Message);
    }

    [Theory]
    [InlineData("1114447773")] // 10 dígitos
    [InlineData("111444777350")] // 12 dígitos
    [InlineData("11144477736")] // dígito verificador errado
    [InlineData("11144477745")] // dígito verificador errado
    [InlineData("00000000000")] // todos os dígitos iguais
    [InlineData("11111111111")] // todos os dígitos iguais
    [InlineData("abc.def.ghi-jk")] // sem dígitos suficientes
    public void AoCriarCpfComFormatoInvalidoDeveLancarValidationException(string valor)
    {
        // Act & Assert
        ValidationException ex = Assert.Throws<ValidationException>(() => new Cpf(valor));
        Assert.Equal("O CPF informado é inválido.", ex.Message);
    }

    [Fact]
    public void AoCriarCpfComPontuacaoDeveNormalizarRemovendoPontuacao()
    {
        // Arrange
        const string cpfComPontuacao = "111.444.777-35";

        // Act
        Cpf cpf = new(cpfComPontuacao);

        // Assert
        Assert.Equal("11144477735", cpf.Valor);
        Assert.Equal("11144477735", cpf.ToString());
    }

    [Fact]
    public void AoCompararCpfsIguaisDeveSerIgualPorValor()
    {
        // Arrange
        Cpf cpf1 = new("111.444.777-35");
        Cpf cpf2 = new("11144477735");

        // Act & Assert
        Assert.Equal(cpf1, cpf2);
        Assert.True(cpf1 == cpf2);
        Assert.False(cpf1 != cpf2);
    }

    [Fact]
    public void AoCompararCpfsIguaisDevemTerMesmoHashCode()
    {
        // Arrange
        Cpf cpf1 = new("111.444.777-35");
        Cpf cpf2 = new("11144477735");

        // Act & Assert
        Assert.Equal(cpf1.GetHashCode(), cpf2.GetHashCode());
    }

    [Fact]
    public void AoCompararCpfsDiferentesDeveSerDiferentePorValor()
    {
        // Arrange
        Cpf cpf1 = new("11144477735");
        Cpf cpf2 = new("52998224725");

        // Act & Assert
        Assert.NotEqual(cpf1, cpf2);
        Assert.False(cpf1 == cpf2);
        Assert.True(cpf1 != cpf2);
    }

    [Fact]
    public void AoCompararCpfComNullDeveRetornarFalse()
    {
        // Arrange
        Cpf cpf = new("11144477735");

        // Act & Assert
        Assert.False(cpf == null);
        Assert.True(cpf != null);
    }

    [Theory]
    [InlineData("11144477735", true)]
    [InlineData("111.444.777-35", true)]
    [InlineData("11144477736", false)]
    [InlineData("00000000000", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void AoChamarEhValidoDeveRetornarResultadoCorreto(string? valor, bool esperado)
    {
        // Act
        bool resultado = Cpf.EhValido(valor!);

        // Assert
        Assert.Equal(esperado, resultado);
    }
}
