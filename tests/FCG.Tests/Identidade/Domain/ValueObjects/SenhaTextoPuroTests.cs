using FCG.Domain.Identidade.ValueObjects;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Tests.Identidade.Domain.ValueObjects;

public class SenhaTextoPuroTests
{
    [Theory]
    [InlineData("Senha123!")]
    [InlineData("senha@123A")]
    public void CriarSenha_Valida_DeveAceitar(string senhaValida)
    {
        // Act
        SenhaTextoPuro senha = new(senhaValida);
        
        // Assert
        Assert.Equal(senhaValida, senha.ToString());
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void CriarSenha_VaziaOuNula_DeveLancarValidationException(string? senhaVazia)
    {
        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaVazia!));
        Assert.Equal("Senha obrigatória", exception.Message);
    }
    
    [Fact]
    public void CriarSenha_ComTamanhoMinimo_DeveAceitar()
    {
        // Arrange
        string senhaCurta = new string('a', SenhaTextoPuro.TAMANHO_MINIMO_SENHA - 3) + "A1!";

        // Act
        SenhaTextoPuro senha = new(senhaCurta);
        
        // Assert
        Assert.Equal(senhaCurta, senha.ToString());
    }

    [Fact]
    public void CriarSenha_MenorQueMinimo_DeveLancarValidationException()
    {
        // Arrange
        const string senhaCurta = "Short1!";

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaCurta));
        Assert.Equal("Senha deve ter no mínimo 8 caracteres", exception.Message);
    }

    [Fact]
    public void CriarSenha_ComTamanhoMaximo_DeveAceitar()
    {
        // Arrange
        string senhaLonga = new string('a', SenhaTextoPuro.TAMANHO_MAXIMO_SENHA - 3) + "A1!";

        // Act
        SenhaTextoPuro senha = new(senhaLonga);
        
        // Assert
        Assert.Equal(senhaLonga, senha.ToString());
    }
    
    [Fact]
    public void CriarSenha_MaiorQueMaximo_DeveLancarValidationException()
    {
        // Arrange
        string senhaLonga = new string('a', SenhaTextoPuro.TAMANHO_MAXIMO_SENHA - 2) + "A1!";

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaLonga));
        Assert.Equal("Senha muito longa", exception.Message);
    }
    
    [Fact]
    public void CriarSenha_SemMaiusculas_DeveLancarValidationException()
    {
        // Arrange
        const string senhaSemMaiuscula = "senhateste12345!";

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaSemMaiuscula));
        Assert.Equal("A senha deve conter ao menos uma letra maiúscula", exception.Message);
    }

    [Fact]
    public void CriarSenha_SemMinusculas_DeveLancarValidationException()
    {
        // Arrange
        const string senhaSemMinuscula = "SENHATESTE12345!";

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaSemMinuscula));
        Assert.Equal("A senha deve conter ao menos uma letra minúscula", exception.Message);
    }
    
    [Fact]
    public void CriarSenha_SemNumeros_DeveLancarValidationException()
    {
        // Arrange
        const string senhaSemNumero = "SenhaSemNumero!";

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaSemNumero));
        Assert.Equal("A senha deve conter ao menos um número", exception.Message);
    }

    [Fact]
    public void CriarSenha_SemCaracteresEspeciais_DeveLancarValidationException()
    {
        // Arrange
        const string senhaSemEspecial = "SenhaTeste12345";

        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaSemEspecial));
        Assert.Equal("A senha deve conter ao menos um caractere especial", exception.Message);
    }
    
    [Fact]
    public void SenhasIguais_DeveSerIgualPorValor()
    {
        // Arrange
        SenhaTextoPuro senha1 = new("SenhaTeste123@");
        SenhaTextoPuro senha2 = new("SenhaTeste123@");

        // Act & Assert
        Assert.Equal(senha1, senha2);
        Assert.True(senha1 == senha2);
        Assert.False(senha1 != senha2);
    }
    
    [Fact]
    public void SenhasIguais_DevemTerMesmoHashCode()
    {
        // Arrange
        SenhaTextoPuro senha1 = new("SenhaTeste123@");
        SenhaTextoPuro senha2 = new("SenhaTeste123@");

        // Act & Assert
        Assert.Equal(senha1.GetHashCode(), senha1.GetHashCode());
    }
    
    [Fact]
    public void SenhasDiferentes_DeveSerDiferentePorValor()
    {
        // Arrange
        SenhaTextoPuro senha1 = new("SenhaTeste123@");
        SenhaTextoPuro senha2 = new("SenhaTeste123!");

        // Act & Assert
        Assert.NotEqual(senha1, senha2);
        Assert.False(senha1 == senha2);
        Assert.True(senha1 != senha2);
    }

    [Fact]
    public void CompararSenhaComNull_DeveRetornarFalse()
    {
        // Arrange
        SenhaTextoPuro senha = new("SenhaTeste123@");

        // Act & Assert
        Assert.False(senha == null);
        Assert.True(senha != null);
    }

    [Theory]
    [InlineData("Senha123!", "Senha123!")]
    [InlineData("senha@123A", "senha@123A")]
    public void CriarSenhaComConfirmacao_ConfirmacaoIgual_DeveAceitar(string senhaValida, string confirmacaoSenha)
    {
        // Act
        SenhaTextoPuro senha = new(senhaValida, confirmacaoSenha);
        
        // Assert
        Assert.Equal(senhaValida, senha.ToString());
    }
    
    [Theory]
    [InlineData("Senha123!", "SEnha123!")]
    [InlineData("senha@123A", "sEnha@123A")]
    public void CriarSenhaComConfirmacao_ConfirmacaoDiferente__DeveLancarValidationException(string senhaValida, string confirmacaoSenha)
    {
        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaTextoPuro(senhaValida, confirmacaoSenha));
        Assert.Equal("'Senha' e 'Confirmação de Senha' são diferentes", exception.Message);
    }
}