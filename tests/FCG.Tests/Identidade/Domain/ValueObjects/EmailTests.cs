using FCG.Domain.Identidade.ValueObjects;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Tests.Identidade.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("joao@exemplo.com")]
    [InlineData("JOAO@EXEMPLO.COM")]
    [InlineData("joao.silva-123@sub.dominio.org")]
    [InlineData("a_b.c-d@dominio.co")]
    [InlineData("joao@a.com")]
    [InlineData("joao.silva@domi-nio.com")]
    public void CriarEmail_Valido_DeveSerCriadoComSucesso(string valor)
    {
        // Act
        Email email = new(valor);

        // Assert
        Assert.Equal(valor.Trim(), email.Valor, ignoreCase: true);
        Assert.Equal(valor.Trim(), email.ToString(), ignoreCase: true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CriarEmail_VazioOuNulo_DeveLancarValidationException(string? valor)
    {
        // Act & Assert
        ValidationException ex = Assert.Throws<ValidationException>(() => new Email(valor!));
        Assert.Equal("E-mail não pode ser vazio", ex.Message);
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("joao..silva@exemplo.com")]
    [InlineData("joao--silva@exemplo.com")]
    [InlineData("joao__silva@exemplo.com")]
    [InlineData(".joao@dominio.com")]
    [InlineData("joao_@dominio.com")]
    [InlineData("joao$silva@dominio.com")]
    [InlineData("joao.silva@dominio.c")]
    [InlineData("joao.silva@dominio.c-m")]
    public void CriarEmail_FormatoInvalido_DeveLancarValidationException(string valor)
    {
        // Act & Assert
        ValidationException ex = Assert.Throws<ValidationException>(() => new Email(valor));
        Assert.Equal("O e-mail informado é inválido.", ex.Message);
    }

    [Fact]
    public void CriarEmail_ComTamanhoMaximo_DeveSerCriadoComSucesso()
    {
        // Arrange
        const string dominioEmail = "dominio.com";
        string local = new('a', Email.TAMANHO_MAXIMO_EMAIL - dominioEmail.Length - 1);
        string emailGrande = $"{local}@{dominioEmail}";
       
        // Act
        Email email = new(emailGrande);
        
        // Assert
        Assert.Equal(emailGrande.Trim(), email.Valor, ignoreCase: true);
        Assert.Equal(emailGrande.Trim(), email.ToString(), ignoreCase: true);
    }
    
    [Fact]
    public void CriarEmail_ExcedeTamanhoMaximo_DeveLancarValidationException()
    {
        // Arrange
        const string dominioEmail = "dominio.com";
        string local = new('a', Email.TAMANHO_MAXIMO_EMAIL - dominioEmail.Length);
        string emailGrande = $"{local}@{dominioEmail}";
       
        // Act & Assert
        ValidationException ex = Assert.Throws<ValidationException>(() => new Email(emailGrande));
        Assert.Contains("E-mail muito longo", ex.Message);
    }

    [Fact]
    public void EmailsIguais_DeveSerIgualPorValor()
    {
        // Arrange
        Email email1 = new("Usuario@Dominio.com");
        Email email2 = new("usuario@dominio.com");

        // Act & Assert
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }
    
    [Fact]
    public void EmailsIguais_DevemTerMesmoHashCode()
    {
        // Arrange
        Email email1 = new("User@Dominio.com");
        Email email2 = new("user@dominio.com");

        // Act & Assert
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    public void EmailsDiferentes_DeveSerDiferentePorValor()
    {
        // Arrange
        Email email1 = new("user1@dominio.com");
        Email email2 = new("user2@dominio.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
        Assert.False(email1 == email2);
        Assert.True(email1 != email2);
    }

    [Fact]
    public void CompararEmailComNull_DeveRetornarFalse()
    {
        // Arrange
        Email email = new("usuario@dominio.com");

        // Act & Assert
        Assert.False(email == null);
        Assert.True(email != null);
    }
    
    [Fact]
    public void Email_NormalizaCorretamente()
    {
        // Arrange
        const string rawEmail = "  JoAo.SiLvA@ExEmPlo.CoM  ";
        var email = new Email(rawEmail);

        // Act
        const string esperado = "joao.silva@exemplo.com";

        // Assert
        Assert.Equal(esperado, email.Valor);
        Assert.Equal(esperado, email.ToString());
    }
}