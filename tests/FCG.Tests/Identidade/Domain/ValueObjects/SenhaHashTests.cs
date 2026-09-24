using FCG.Domain.Identidade.ValueObjects;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Tests.Identidade.Domain.ValueObjects;

public class SenhaHashTests
{
    [Fact]
    public void CriarSenhaHash_Valida_DeveAceitar()
    {
        // Arrange
        string senhaValida = new('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH);
        
        // Act
        SenhaHash senhaHash = new(senhaValida);

        // Assert
        Assert.Equal(senhaValida, senhaHash.Senha);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void CriarSenhaHash_VaziaOuNula_DeveLancarValidationException(string? senhaVazia)
    {
        // Act & Assert
        ValidationException exception = Assert.Throws<ValidationException>(() => new SenhaHash(senhaVazia!));
        Assert.Equal("Senha não pode ser vazia", exception.Message);
    }
    
    [Fact]
    public void CriarSenhaHash_TamanhoIncorreto_DeveLancarValidationException()
    {
        // Arrange
        string senhaComTamanhoIncorreto1 = new('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH - 1);
        string senhaComTamanhoIncorreto2 = new('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH + 1);

        // Act & Assert
        ValidationException exception1 = Assert.Throws<ValidationException>(() => new SenhaHash(senhaComTamanhoIncorreto1));
        Assert.Equal("Senha hash com tamanho inesperado", exception1.Message);
        ValidationException exception2 = Assert.Throws<ValidationException>(() => new SenhaHash(senhaComTamanhoIncorreto2));
        Assert.Equal("Senha hash com tamanho inesperado", exception2.Message);
    }
    
    [Fact]
    public void SenhasIguais_DeveSerIgualPorValor()
    {
        // Arrange
        SenhaHash senha1 = new(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));
        SenhaHash senha2 = new(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));

        // Act & Assert
        Assert.Equal(senha1, senha2);
        Assert.True(senha1 == senha2);
        Assert.False(senha1 != senha2);
    }
    
    [Fact]
    public void SenhasIguais_DevemTerMesmoHashCode()
    {
        // Arrange
        SenhaHash senha1 = new(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));
        SenhaHash senha2 = new(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));

        // Act & Assert
        Assert.Equal(senha1.GetHashCode(), senha2.GetHashCode());
    }
    
    [Fact]
    public void SenhasDiferentes_DeveSerDiferentePorValor()
    {
        // Arrange
        SenhaHash senha1 = new(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));
        SenhaHash senha2 = new(new string('b', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));

        // Act & Assert
        Assert.NotEqual(senha1, senha2);
        Assert.False(senha1 == senha2);
        Assert.True(senha1 != senha2);
    }

    [Fact]
    public void CompararSenhaComNull_DeveRetornarFalse()
    {
        // Arrange
        SenhaHash senha = new(new string('a', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH));

        // Act & Assert
        Assert.False(senha == null);
        Assert.True(senha != null);
    }
}